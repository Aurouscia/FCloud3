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
        .au-floated-paras .para,
        .au-floated-paras .indent {
            overflow-x: initial !important;
        }
        .au-floated-paras .para .quote {
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

interface TableState {
    table: HTMLTableElement
    placeholder: Comment
    direction: 'left' | 'right'
    mobile: MobileConfig
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

function parseTriggerCell(text: string): { triggered: boolean; direction: 'left' | 'right'; mobile: MobileConfig } {
    const parts = text.split(/\s+/).filter(Boolean)
    if (parts.length === 0 || !getTriggerPattern().test(parts[0])) {
        return { triggered: false, direction: 'right', mobile: defaultMobileConfig }
    }
    const rest = parts.slice(1).join(' ')
    return {
        triggered: true,
        direction: parseFloatDirection(rest),
        mobile: parseMobileConfig(rest)
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
        // 先移回原始位置，确保按钮出现在表格初始位置而非 .paras 顶部
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
                showErrorAfter(state.table, '找不到.paras元素')
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
            showErrorAfter(state.table, '找不到.paras元素')
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

        for (const row of Array.from(table.rows)) {
            for (const cell of Array.from(row.cells)) {
                const text = cell.textContent?.trim() ?? ''
                const result = parseTriggerCell(text)
                if (result.triggered) {
                    triggered = true
                    direction = result.direction
                    mobile = result.mobile
                    row.remove()
                    break
                }
            }
        }

        if (triggered) {
            const placeholder = document.createComment('au-table-float-placeholder')
            table.parentElement?.insertBefore(placeholder, table)

            const state: TableState = {
                table,
                placeholder,
                direction,
                mobile,
                currentMode: 'original'
            }
            tableStates.set(table, state)
            updateTablePosition(state, paras)
        }
    }
}
