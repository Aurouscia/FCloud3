import { triggers } from '../public/options.json'
import { applyDrawer } from './drawer'

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
            overflow-x: unset;
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

interface MobileConfig {
    strategy: MobileStrategy
    threshold: number
    buttonText: string
}

const defaultMobileConfig: MobileConfig = { strategy: 'asis', threshold: 500, buttonText: '打开表格' }

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

function shouldApplyFloat(mobile: MobileConfig): boolean {
    if (mobile.strategy === 'enforce') {
        return true
    }
    return !isMobile(mobile)
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

function applyFloatStyles(table: HTMLTableElement, direction: 'left' | 'right') {
    table.style.float = direction
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

export function run() {
    ensureStyles()
    const paras = findFirstParasInWikiView()
    const tables = Array.from(document.getElementsByTagName('table'))
    for (const table of tables) {
        for (const row of Array.from(table.rows)) {
            let triggered = false
            let direction: 'left' | 'right' = 'right'
            let mobile = defaultMobileConfig
            for (const cell of Array.from(row.cells)) {
                const text = cell.textContent?.trim() ?? ''
                const result = parseTriggerCell(text)
                if (result.triggered) {
                    triggered = true
                    direction = result.direction
                    mobile = result.mobile
                    break
                }
            }
            if (triggered) {
                row.remove()
                if (mobile.strategy === 'drawer' && isMobile(mobile)) {
                    applyDrawer(table, direction, mobile.buttonText)
                }
                else if (shouldApplyFloat(mobile)) {
                    table.classList.add('au-floated-table')
                    applyFloatStyles(table, direction)
                    if (paras) {
                        paras.prepend(table)
                        paras.classList.add('au-floated-paras')
                    }
                    else {
                        showErrorAfter(table, '找不到.paras元素')
                    }
                }
            }
        }
    }
}
