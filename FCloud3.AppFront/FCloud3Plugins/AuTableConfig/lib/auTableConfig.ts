import { triggers } from '../public/options.json'

const triggerPattern = new RegExp(`^(${triggers.join('|')})$`)

interface ColumnConfig {
    width?: string
    nowrap: boolean
    textAlign?: string
    verticalAlign?: string
}

export function run() {
    const tables = document.getElementsByTagName('table')
    for (const t of tables) {
        const rows = t.rows
        if (rows.length === 0) {
            continue
        }
        const firstRow = rows[0]
        const firstCell = firstRow.cells[0]
        if (!firstCell) {
            continue
        }
        const firstCellText = firstCell.innerText.trim()
        const parts = firstCellText.split(/\s+/)
        if (parts.length < 2 || !triggerPattern.test(parts[0])) {
            continue
        }

        const configs: ColumnConfig[] = []
        for (let c = 0; c < firstRow.cells.length; c++) {
            const cell = firstRow.cells[c]
            const text = cell.innerText.trim()
            const cellParts = text.split(/\s+/)
            const configText = cellParts.slice(1).join(' ')
            configs.push(parseConfig(configText))
        }

        for (let c = 0; c < configs.length; c++) {
            const config = configs[c]
            for (let r = 1; r < rows.length; r++) {
                const cell = rows[r].cells[c]
                if (!cell) {
                    continue
                }
                if (config.width && r === 1) {
                    cell.style.width = config.width
                }
                if (config.nowrap) {
                    cell.style.whiteSpace = 'nowrap'
                }
                if (config.textAlign) {
                    cell.style.textAlign = config.textAlign
                }
                if (config.verticalAlign) {
                    cell.style.verticalAlign = config.verticalAlign
                }
            }
        }

        firstRow.remove()
    }
}

function parseConfig(text: string): ColumnConfig {
    const config: ColumnConfig = { nowrap: false }
    const parts = text.split(/[,，\s]+/).map(s => s.trim()).filter(Boolean)
    for (const part of parts) {
        const lower = part.toLowerCase()
        if (lower === 'nowrap' || lower === '不换行') {
            config.nowrap = true
        }
        else if (/^\d+(px|em|%|rem|vw|vh|ch|ex|cm|mm|in|pt|pc)?$/.test(part)) {
            config.width = part
        }
        else if (['left', 'center', 'right', 'justify'].includes(lower)) {
            config.textAlign = lower
        }
        else if (['top', 'middle', 'bottom'].includes(lower)) {
            config.verticalAlign = lower === 'middle' ? 'middle' : lower
        }
    }
    return config
}
