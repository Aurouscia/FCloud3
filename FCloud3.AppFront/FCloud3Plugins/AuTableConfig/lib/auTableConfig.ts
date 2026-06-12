import { triggers } from '../public/options.json'

function getTriggerPattern() {
    return new RegExp(`^(${triggers.join('|')})$`)
}

const tableWidthPattern = /^table:(?:(min|max):)?(\d+(?:px|em|%|rem|vw|vh|ch|ex|cm|mm|in|pt|pc)?)$/i

interface ColumnConfig {
    width?: string
    widthMode?: 'exact' | 'min' | 'max'
    nowrap: boolean
    textAlign?: string
    verticalAlign?: string
}

interface TableWidthConfig {
    width: string
    mode?: 'min' | 'max'
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
        const firstCellText = firstCell.textContent?.trim() ?? ''
        const parts = firstCellText.split(/\s+/)
        if (parts.length < 2 || !getTriggerPattern().test(parts[0])) {
            continue
        }

        // 解析第一列中 table:xxx 的表格宽度设置
        const firstColConfigText = parts.slice(1).join(' ')
        const tableWidthConfig = parseTableWidth(firstColConfigText)
        if (tableWidthConfig) {
            applyTableWidth(t, tableWidthConfig)
        }

        const configs: ColumnConfig[] = []
        for (let c = 0; c < firstRow.cells.length; c++) {
            const cell = firstRow.cells[c]
            const text = cell.textContent?.trim() ?? ''
            const cellParts = text.split(/\s+/)
            const configText = c === 0 ? cellParts.slice(1).join(' ') : text
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
                    applyColumnWidth(cell, config.width, config.widthMode ?? 'exact')
                }
                if (config.nowrap) {
                    applyNowrap(cell)
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

function parseTableWidth(text: string): TableWidthConfig | undefined {
    const parts = text.split(/[,，\s]+/).map(s => s.trim()).filter(Boolean)
    for (const part of parts) {
        const match = tableWidthPattern.exec(part)
        if (match) {
            return {
                mode: match[1] as 'min' | 'max' | undefined,
                width: match[2]
            }
        }
    }
    return undefined
}

function applyTableWidth(table: HTMLTableElement, config: TableWidthConfig) {
    if (config.mode === 'min') {
        table.style.minWidth = config.width
    }
    else if (config.mode === 'max') {
        table.style.maxWidth = config.width
    }
    else {
        table.style.width = config.width
    }
}

/** 
 * 递归地应用nowrap
 * 由于本系统中，单元格内的换行符会被处理为多个p标签，所以这样做是正确的，也无需提供 pre-line 等其他选项
 */
function applyNowrap(element: HTMLElement) {
    element.style.whiteSpace = 'nowrap'
    for (const child of element.children) {
        applyNowrap(child as HTMLElement)
    }
}

function applyColumnWidth(cell: HTMLTableCellElement, width: string, mode: 'exact' | 'min' | 'max') {
    if (mode === 'exact') {
        cell.style.width = width
        cell.style.minWidth = width
        cell.style.maxWidth = width
    }
    else if (mode === 'min') {
        cell.style.minWidth = width
    }
    else if (mode === 'max') {
        cell.style.maxWidth = width
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
        else if (['left', 'center', 'right', 'justify'].includes(lower)) {
            config.textAlign = lower
        }
        else if (['top', 'middle', 'bottom'].includes(lower)) {
            config.verticalAlign = lower === 'middle' ? 'middle' : lower
        }
        else if (!tableWidthPattern.test(part)) {
            const widthMatch = /^(min|max):(\d+(?:px|em|%|rem|vw|vh|ch|ex|cm|mm|in|pt|pc)?)$/.exec(part)
            if (widthMatch) {
                config.widthMode = widthMatch[1] as 'min' | 'max'
                config.width = widthMatch[2]
            }
            else if (/^\d+(px|em|%|rem|vw|vh|ch|ex|cm|mm|in|pt|pc)?$/.test(part)) {
                config.width = part
                config.widthMode = 'exact'
            }
        }
    }
    return config
}
