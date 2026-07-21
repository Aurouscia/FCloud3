import { triggers } from '../public/options.json'
import { applyDrawer, removeDrawer } from './drawer'

function getTriggerPattern() {
    return new RegExp(`^(${triggers.join('|')})$`)
}

function findFirstParasInWikiView(): Element | null {
    const wikiView = document.querySelector('.wikiView')
    return wikiView?.querySelector('.paras') ?? null
}

function showErrorAfter(table: HTMLTableElement, message: string) {
    const error = document.createElement('b')
    error.style.color = 'red'
    error.textContent = message
    table.after(error)
}

const styleElementId = 'au-table-float-styles'

function ensureStyles() {
    if (document.getElementById(styleElementId)) {
        return
    }
    const style = document.createElement('style')
    style.id = styleElementId
    style.textContent = `
        .wikiView .au-floated-paras .para,
        .wikiView .au-floated-paras .para .indent {
            overflow-x: initial;
        }
        .wikiView .au-floated-paras .para .quote {
            overflow: hidden; /*确保创建BFC，避免quote占满整行（它需要被压缩）*/
        }
        .indent.indentFolded {
            display: none !important
        }
        .au-table-float-drawer {
            padding: 6px;
        }
        .au-table-float-drawer table {
            min-width: 100%;
            font-size: 14px
        }
    `
    document.head.appendChild(style)
}

type MobileStrategy = 'asis' | 'enforce' | 'drawer'
type DisplayMode = 'original' | 'float' | 'drawer'

interface MobileConfig {
    strategy: MobileStrategy
    threshold: number
    buttonText: string
}

const defaultMobileConfig: MobileConfig = { strategy: 'asis', threshold: 800, buttonText: '打开表格' }

interface CssStyles {
    fontSize?: string
    maxWidth?: string
    minWidth?: string
    width?: string
}

interface TargetConfig {
    keyword: string
    position: 'before' | 'after'
}

interface TargetPConfig {
    keyword: string
    position: 'before' | 'after'
}

interface TableState {
    table: HTMLTableElement
    placeholder: Comment
    direction: 'left' | 'right'
    mobile: MobileConfig
    cssStyles: CssStyles
    target: TargetConfig | null
    targetP: TargetPConfig | null
    currentMode: DisplayMode
}

const tableStates = new Map<HTMLTableElement, TableState>()
let resizeListenerRegistered = false

function parseMobileConfig(text: string): MobileConfig {
    const match = /mobile\(([^)]*)\)/i.exec(text)
    if (!match) {
        return defaultMobileConfig
    }
    const inner = match[1].trim()
    if (!inner) {
        return defaultMobileConfig
    }
    const parts = inner.split(',').map(s => s.trim())
    const strategy = parts[0]?.toLowerCase() ?? ''
    const thresholdStr = parts[1] ?? ''
    const buttonText = parts[2]?.trim() || defaultMobileConfig.buttonText
    const threshold = thresholdStr ? parseInt(thresholdStr, 10) : defaultMobileConfig.threshold
    return {
        strategy: ['asis', 'enforce', 'drawer'].includes(strategy) ? strategy as MobileStrategy : defaultMobileConfig.strategy,
        threshold: Number.isNaN(threshold) ? defaultMobileConfig.threshold : threshold,
        buttonText
    }
}

function isMobile(mobile: MobileConfig): boolean {
    return window.innerWidth < mobile.threshold
}

function parseFloatDirection(text: string): 'left' | 'right' {
    const lower = text.toLowerCase()
    if (/\bleft\b/.test(lower)) {
        return 'left'
    }
    if (/\bright\b/.test(lower)) {
        return 'right'
    }
    return 'right'
}

function parseCssStyles(text: string): CssStyles {
    const styles: CssStyles = {}
    const mappings: { key: keyof CssStyles; prefix: string }[] = [
        { key: 'fontSize', prefix: 'font-size' },
        { key: 'maxWidth', prefix: 'max-width' },
        { key: 'minWidth', prefix: 'min-width' },
        { key: 'width', prefix: 'width' }
    ]
    for (const { key, prefix } of mappings) {
        const match = new RegExp(`\\b${prefix}\\(([^)]*)\\)`, 'i').exec(text)
        const value = match?.[1].trim()
        if (value) {
            styles[key] = value
        }
    }
    return styles
}

function applyCssStyles(table: HTMLTableElement, styles: CssStyles) {
    if (styles.fontSize) {
        table.style.fontSize = styles.fontSize
    }
    if (styles.maxWidth) {
        table.style.maxWidth = styles.maxWidth
    }
    if (styles.minWidth) {
        table.style.minWidth = styles.minWidth
    }
    if (styles.width) {
        table.style.width = styles.width
    }
}

function parseTargetConfig(text: string): TargetConfig | null {
    const match = /target\(([^)]*)\)/i.exec(text)
    if (!match) {
        return null
    }
    const inner = match[1].trim()
    if (!inner) {
        return null
    }
    const parts = inner.split(',').map(s => s.trim())
    const keyword = parts[0]
    if (!keyword) {
        return null
    }
    const position = parts[1]?.toLowerCase()
    return {
        keyword,
        position: position === 'after' ? 'after' : 'before'
    }
}

function parseTargetPConfig(text: string): TargetPConfig | null {
    const match = /target-p\(([^)]*)\)/i.exec(text)
    if (!match) {
        return null
    }
    const inner = match[1].trim()
    if (!inner) {
        return null
    }
    const parts = inner.split(',').map(s => s.trim())
    const keyword = parts[0]
    if (!keyword) {
        return null
    }
    const position = parts[1]?.toLowerCase()
    return {
        keyword,
        position: position === 'before' ? 'before' : 'after'
    }
}

function findTargetElement(keyword: string): HTMLElement | null {
    const h1s = document.querySelectorAll('h1')
    for (const h1 of Array.from(h1s)) {
        if (h1.textContent?.includes(keyword)) {
            return h1
        }
    }
    return null
}

function findMatchingP(keyword: string): HTMLElement | null {
    const ps = document.querySelectorAll('p')
    for (const p of Array.from(ps)) {
        if (p.textContent?.includes(keyword)) {
            return p
        }
    }
    return null
}

function insertTableInsideP(table: HTMLTableElement, p: HTMLElement, keyword: string, position: 'before' | 'after'): boolean {
    const walker = document.createTreeWalker(p, NodeFilter.SHOW_TEXT, null)
    let currentNode: Text | null
    while ((currentNode = walker.nextNode() as Text)) {
        const text = currentNode.textContent || ''
        const index = text.indexOf(keyword)
        if (index >= 0) {
            const offset = position === 'before' ? index : index + keyword.length
            const afterText = currentNode.splitText(offset)
            currentNode.parentNode?.insertBefore(table, afterText)
            return true
        }
    }
    return false
}

function parseTriggerCell(text: string): { triggered: boolean; direction: 'left' | 'right'; mobile: MobileConfig; cssStyles: CssStyles; target: TargetConfig | null; targetP: TargetPConfig | null } {
    const parts = text.split(/\s+/).filter(Boolean)
    if (parts.length === 0 || !getTriggerPattern().test(parts[0])) {
        return { triggered: false, direction: 'right', mobile: defaultMobileConfig, cssStyles: {}, target: null, targetP: null }
    }
    const rest = parts.slice(1).join(' ')
    return {
        triggered: true,
        direction: parseFloatDirection(rest),
        mobile: parseMobileConfig(rest),
        cssStyles: parseCssStyles(rest),
        target: parseTargetConfig(rest),
        targetP: parseTargetPConfig(rest)
    }
}

function clearFloatStyles(table: HTMLTableElement) {
    table.style.float = ''
    table.style.clear = ''
    table.style.marginLeft = ''
    table.style.marginRight = ''
    table.style.marginBottom = ''
    table.style.marginTop = ''
}

function applyFloatStyles(table: HTMLTableElement, direction: 'left' | 'right') {
    table.style.float = direction
    table.style.clear = direction
    if (direction === 'right') {
        table.style.marginLeft = '10px'
        table.style.marginRight = ''
    }
    else {
        table.style.marginRight = '10px'
        table.style.marginLeft = ''
    }
    table.style.marginBottom = '10px'
    table.style.marginTop = ''
}

function moveToOriginal(state: TableState) {
    if (state.currentMode === 'drawer') {
        removeDrawer(state.table)
    }
    state.placeholder.parentElement?.insertBefore(state.table, state.placeholder)
    state.table.classList.remove('au-floated-table')
    clearFloatStyles(state.table)
    state.currentMode = 'original'
}

function moveToFloat(state: TableState, paras: Element | null): boolean {
    if (state.currentMode === 'drawer') {
        removeDrawer(state.table)
    }
    state.table.classList.add('au-floated-table')
    applyFloatStyles(state.table, state.direction)

    if (state.target) {
        const target = findTargetElement(state.target.keyword)
        if (!target) {
            return false
        }
        if (state.target.position === 'before') {
            target.parentElement?.insertBefore(state.table, target)
        }
        else {
            target.after(state.table)
        }
        state.currentMode = 'float'
        return true
    }

    if (state.targetP) {
        const p = findMatchingP(state.targetP.keyword)
        if (!p) {
            return false
        }
        const inserted = insertTableInsideP(state.table, p, state.targetP.keyword, state.targetP.position)
        if (!inserted) {
            return false
        }
        state.currentMode = 'float'
        return true
    }

    if (!paras) {
        return false
    }
    paras.prepend(state.table)
    paras.classList.add('au-floated-paras')
    state.currentMode = 'float'
    return true
}

function moveToDrawer(state: TableState) {
    if (state.currentMode === 'float') {
        state.table.classList.remove('au-floated-table')
        clearFloatStyles(state.table)
        // 先移回原始位置，确保按钮出现在表格初始位置而非 .paras 或 target 元素旁边
        state.placeholder.parentElement?.insertBefore(state.table, state.placeholder)
    }
    applyDrawer(state.table, state.direction, state.mobile.buttonText)
    state.currentMode = 'drawer'
}

function updateTablePosition(state: TableState, paras: Element | null) {
    if (state.mobile.strategy === 'enforce') {
        if (state.currentMode !== 'float') {
            moveToFloat(state, paras)
        }
        return
    }

    const nowMobile = isMobile(state.mobile)

    if (state.mobile.strategy === 'drawer') {
        if (nowMobile && state.currentMode !== 'drawer') {
            moveToDrawer(state)
        }
        else if (!nowMobile && state.currentMode !== 'float') {
            const moved = moveToFloat(state, paras)
            if (!moved) {
                const message = state.targetP
                    ? `找不到包含"${state.targetP.keyword}"的p元素`
                    : state.target
                        ? `找不到包含"${state.target.keyword}"的h1元素`
                        : '找不到.paras元素'
                showErrorAfter(state.table, message)
            }
        }
        return
    }

    // asis
    if (nowMobile && state.currentMode === 'float') {
        moveToOriginal(state)
    }
    else if (!nowMobile && state.currentMode !== 'float') {
        const moved = moveToFloat(state, paras)
        if (!moved) {
            const message = state.targetP
                ? `找不到包含"${state.targetP.keyword}"的p元素`
                : state.target
                    ? `找不到包含"${state.target.keyword}"的h1元素`
                    : '找不到.paras元素'
            showErrorAfter(state.table, message)
        }
    }
}

function onResize() {
    const paras = findFirstParasInWikiView()
    for (const state of tableStates.values()) {
        if (!state.table.isConnected) {
            continue
        }
        updateTablePosition(state, paras)
    }
}

function ensureResizeListener() {
    if (resizeListenerRegistered) {
        return
    }
    window.addEventListener('resize', onResize)
    resizeListenerRegistered = true
}

export function run() {
    ensureStyles()
    ensureResizeListener()
    const paras = findFirstParasInWikiView()
    const tables = Array.from(document.getElementsByTagName('table'))

    for (const table of tables) {
        if (tableStates.has(table)) {
            continue
        }

        let triggered = false
        let direction: 'left' | 'right' = 'right'
        let mobile = defaultMobileConfig
        let cssStyles: CssStyles = {}
        let target: TargetConfig | null = null
        let targetP: TargetPConfig | null = null

        for (const row of Array.from(table.rows)) {
            for (const cell of Array.from(row.cells)) {
                const text = cell.textContent?.trim() ?? ''
                const result = parseTriggerCell(text)
                if (result.triggered) {
                    triggered = true
                    direction = result.direction
                    mobile = result.mobile
                    cssStyles = result.cssStyles
                    target = result.target
                    targetP = result.targetP
                    row.remove()
                    break
                }
            }
        }

        if (triggered) {
            const placeholder = document.createComment('au-table-float-placeholder')
            table.parentElement?.insertBefore(placeholder, table)

            applyCssStyles(table, cssStyles)

            const state: TableState = {
                table,
                placeholder,
                direction,
                mobile,
                cssStyles,
                target,
                targetP,
                currentMode: 'original'
            }
            tableStates.set(table, state)
            updateTablePosition(state, paras)
        }
    }
}
