import { triggers } from '../public/options.json'

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
    `
    document.head.appendChild(style)
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

function parseTriggerCell(text: string): { triggered: boolean; direction: 'left' | 'right' } {
    const parts = text.split(/\s+/).filter(Boolean)
    if (parts.length === 0 || !getTriggerPattern().test(parts[0])) {
        return { triggered: false, direction: 'right' }
    }
    const rest = parts.slice(1).join(' ')
    return { triggered: true, direction: parseFloatDirection(rest) }
}

export function run() {
    ensureStyles()
    const paras = findFirstParasInWikiView()
    const tables = Array.from(document.getElementsByTagName('table'))
    for (const table of tables) {
        for (const row of Array.from(table.rows)) {
            let triggered = false
            let direction: 'left' | 'right' = 'right'
            for (const cell of Array.from(row.cells)) {
                const text = cell.textContent?.trim() ?? ''
                const result = parseTriggerCell(text)
                if (result.triggered) {
                    triggered = true
                    direction = result.direction
                    break
                }
            }
            if (triggered) {
                row.remove()
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
