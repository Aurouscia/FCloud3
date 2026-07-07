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
        .au-floated-table {
            margin: 10px !important;
        }
        .au-floated-paras .para,
        .au-floated-paras .indent {
            overflow-x: unset;
        }
    `
    document.head.appendChild(style)
}

export function run() {
    ensureStyles()
    const paras = findFirstParasInWikiView()
    const tables = Array.from(document.getElementsByTagName('table'))
    for (const table of tables) {
        for (const row of Array.from(table.rows)) {
            let triggered = false
            for (const cell of Array.from(row.cells)) {
                const text = cell.textContent?.trim() ?? ''
                if (getTriggerPattern().test(text)) {
                    triggered = true
                    break
                }
            }
            if (triggered) {
                row.remove()
                table.style.float = 'right'
                table.classList.add('au-floated-table')
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
