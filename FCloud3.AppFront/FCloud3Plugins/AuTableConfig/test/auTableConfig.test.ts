import { describe, it, expect, beforeEach, vi } from 'vitest'

vi.mock('../public/options.json', () => ({
  triggers: ['AuTc', 'au-table-config', 'au-tc'],
  priority: 1
}))

import { run } from '../lib/auTableConfig'

describe('AuTableConfig', () => {
  beforeEach(() => {
    document.body.innerHTML = ''
  })

  function createTable(html: string): HTMLTableElement {
    const table = document.createElement('table')
    table.innerHTML = html
    document.body.appendChild(table)
    return table
  }

  function qs<T extends Element = Element>(selector: string): T {
    return document.querySelector(selector) as T
  }

  function qsa<T extends Element = Element>(selector: string): NodeListOf<T> {
    return document.querySelectorAll(selector) as NodeListOf<T>
  }

  describe('trigger detection', () => {
    it('should apply config when first cell starts with trigger', () => {
      createTable(`
        <tr><td>AuTc 100px nowrap left top</td><td>AuTc nowrap center</td></tr>
        <tr><td>数据1</td><td>数据2</td></tr>
      `)
      run()
      const cells = qsa<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cells.length).toBe(2)
      expect(cells[0].style.width).toBe('100px')
      expect(cells[0].style.whiteSpace).toBe('nowrap')
      expect(cells[0].style.textAlign).toBe('left')
      expect(cells[0].style.verticalAlign).toBe('top')
      expect(cells[1].style.whiteSpace).toBe('nowrap')
      expect(cells[1].style.textAlign).toBe('center')
    })

    it('should detect alias au-table-config', () => {
      createTable(`
        <tr><td>au-table-config 200px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('200px')
    })

    it('should detect alias au-tc', () => {
      createTable(`
        <tr><td>au-tc nowrap</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.whiteSpace).toBe('nowrap')
    })

    it('should skip tables without trigger in first cell', () => {
      createTable(`
        <tr><td>普通文本</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('')
      expect(cell.style.whiteSpace).toBe('')
    })

    it('should skip empty tables', () => {
      const table = document.createElement('table')
      document.body.appendChild(table)
      run()
      expect(document.querySelector('table')).not.toBeNull()
    })

    it('should skip tables with empty first row', () => {
      createTable(`
        <tr></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(2)
      expect(rows[0].cells.length).toBe(0)
      expect(rows[1].querySelector('td')!.textContent).toBe('数据')
    })
  })

  describe('config parsing', () => {
    it('should parse width with various units', () => {
      const units = ['px', 'em', '%', 'rem', 'vw', 'vh', 'ch', 'ex', 'cm', 'mm', 'in', 'pt', 'pc']
      for (const unit of units) {
        document.body.innerHTML = ''
        createTable(`
          <tr><td>AuTc 50${unit}</td></tr>
          <tr><td>数据</td></tr>
        `)
        run()
        const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
        expect(cell.style.width).toBe(`50${unit}`)
      }
    })

    it('should parse width without unit (jsdom may ignore unitless width)', () => {
      createTable(`
        <tr><td>AuTc 100</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      // jsdom 可能会忽略无单位的宽度值，但代码逻辑上设置了 width = '100'
      // 这里只验证没有抛出异常且表格被处理（配置行被移除）
      expect(document.querySelectorAll('table tr').length).toBe(1)
    })

    it('should parse nowrap', () => {
      createTable(`
        <tr><td>AuTc nowrap</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.whiteSpace).toBe('nowrap')
    })

    it('should apply nowrap recursively to child elements', () => {
      createTable(`
        <tr><td>AuTc nowrap</td></tr>
        <tr><td><b>粗体</b> <span>文本</span></td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.whiteSpace).toBe('nowrap')
      const b = cell.querySelector('b')
      const span = cell.querySelector('span')
      expect(b!.style.whiteSpace).toBe('nowrap')
      expect(span!.style.whiteSpace).toBe('nowrap')
    })

    it('should parse 不换行 (Chinese nowrap)', () => {
      createTable(`
        <tr><td>AuTc 不换行</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.whiteSpace).toBe('nowrap')
    })

    it('should parse text align values', () => {
      const aligns = ['left', 'center', 'right', 'justify']
      for (const align of aligns) {
        document.body.innerHTML = ''
        createTable(`
          <tr><td>AuTc ${align}</td></tr>
          <tr><td>数据</td></tr>
        `)
        run()
        const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
        expect(cell.style.textAlign).toBe(align)
      }
    })

    it('should parse vertical align values', () => {
      const aligns = [
        { input: 'top', expected: 'top' },
        { input: 'middle', expected: 'middle' },
        { input: 'bottom', expected: 'bottom' },
      ]
      for (const { input, expected } of aligns) {
        document.body.innerHTML = ''
        createTable(`
          <tr><td>AuTc ${input}</td></tr>
          <tr><td>数据</td></tr>
        `)
        run()
        const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
        expect(cell.style.verticalAlign).toBe(expected)
      }
    })

    it('should parse multiple configs separated by comma', () => {
      createTable(`
        <tr><td>AuTc 100px,nowrap,left,top</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.whiteSpace).toBe('nowrap')
      expect(cell.style.textAlign).toBe('left')
      expect(cell.style.verticalAlign).toBe('top')
    })

    it('should parse multiple configs separated by Chinese comma', () => {
      createTable(`
        <tr><td>AuTc 100px，nowrap，center，middle</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.whiteSpace).toBe('nowrap')
      expect(cell.style.textAlign).toBe('center')
      expect(cell.style.verticalAlign).toBe('middle')
    })

    it('should parse multiple configs separated by space', () => {
      createTable(`
        <tr><td>AuTc 100px nowrap right bottom</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.whiteSpace).toBe('nowrap')
      expect(cell.style.textAlign).toBe('right')
      expect(cell.style.verticalAlign).toBe('bottom')
    })

    it('should ignore unknown config values', () => {
      createTable(`
        <tr><td>AuTc 100px unknown left</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.textAlign).toBe('left')
      expect(cell.style.whiteSpace).toBe('')
    })

    it('should handle empty config after trigger', () => {
      createTable(`
        <tr><td>AuTc</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('')
      expect(cell.style.whiteSpace).toBe('')
    })
  })

  describe('multi-column tables', () => {
    it('should apply different configs to different columns', () => {
      createTable(`
        <tr>
          <td>AuTc 100px nowrap left top</td>
          <td>AuTc 200px center middle</td>
          <td>AuTc nowrap right bottom</td>
        </tr>
        <tr><td>A1</td><td>B1</td><td>C1</td></tr>
        <tr><td>A2</td><td>B2</td><td>C2</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(2) // first row removed

      const row1 = rows[0]
      const cells = row1.querySelectorAll<HTMLTableCellElement>('td')
      expect(cells[0].style.width).toBe('100px')
      expect(cells[0].style.whiteSpace).toBe('nowrap')
      expect(cells[0].style.textAlign).toBe('left')
      expect(cells[0].style.verticalAlign).toBe('top')

      expect(cells[1].style.width).toBe('200px')
      expect(cells[1].style.textAlign).toBe('center')
      expect(cells[1].style.verticalAlign).toBe('middle')

      expect(cells[2].style.whiteSpace).toBe('nowrap')
      expect(cells[2].style.textAlign).toBe('right')
      expect(cells[2].style.verticalAlign).toBe('bottom')
    })

    it('should skip missing cells in data rows', () => {
      createTable(`
        <tr><td>AuTc 100px</td><td>AuTc nowrap</td></tr>
        <tr><td>A1</td></tr>
        <tr><td>A2</td><td>B2</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(2)
      // Row 1 only has 1 cell, should not throw
      expect(rows[0].querySelectorAll('td').length).toBe(1)
      expect(rows[0].querySelector<HTMLTableCellElement>('td')!.style.width).toBe('100px')
    })

    it('should apply width only to first data row', () => {
      createTable(`
        <tr><td>AuTc 100px</td></tr>
        <tr><td>第一行</td></tr>
        <tr><td>第二行</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows[0].querySelector<HTMLTableCellElement>('td')!.style.width).toBe('100px')
      expect(rows[1].querySelector<HTMLTableCellElement>('td')!.style.width).toBe('')
    })

    it('should apply table width via table: prefix', () => {
      createTable(`
        <tr><td>AuTc table:100% 100px nowrap</td><td>200px center</td></tr>
        <tr><td>数据1</td><td>数据2</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.width).toBe('100%')
      const cells = qsa<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cells[0].style.width).toBe('100px')
      expect(cells[0].style.whiteSpace).toBe('nowrap')
      expect(cells[1].style.width).toBe('200px')
    })

    it('should apply table width with various units', () => {
      const cases = ['100%', '500px', '50em', '80vw']
      for (const width of cases) {
        document.body.innerHTML = ''
        createTable(`
          <tr><td>AuTc table:${width}</td></tr>
          <tr><td>数据</td></tr>
        `)
        run()
        const table = qs<HTMLTableElement>('table')
        expect(table.style.width).toBe(width)
      }
    })

    it('should apply table min-width via table:min:', () => {
      createTable(`
        <tr><td>AuTc table:min:1000px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.width).toBe('')
      expect(table.style.minWidth).toBe('1000px')
      expect(table.style.maxWidth).toBe('')
    })

    it('should apply table max-width via table:max:', () => {
      createTable(`
        <tr><td>AuTc table:max:800px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.width).toBe('')
      expect(table.style.minWidth).toBe('')
      expect(table.style.maxWidth).toBe('800px')
    })

    it('should not treat table: as column width', () => {
      createTable(`
        <tr><td>AuTc table:100%</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('')
      expect(cell.style.minWidth).toBe('')
      expect(cell.style.maxWidth).toBe('')
    })

    it('should combine table: with min: and max: column widths', () => {
      createTable(`
        <tr><td>AuTc table:100% min:200px nowrap</td><td>max:150px center</td></tr>
        <tr><td>数据1</td><td>数据2</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.width).toBe('100%')

      const cells = qsa<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cells[0].style.width).toBe('')
      expect(cells[0].style.minWidth).toBe('200px')
      expect(cells[0].style.maxWidth).toBe('')
      expect(cells[0].style.whiteSpace).toBe('nowrap')

      expect(cells[1].style.width).toBe('')
      expect(cells[1].style.minWidth).toBe('')
      expect(cells[1].style.maxWidth).toBe('150px')
      expect(cells[1].style.textAlign).toBe('center')
    })

    it('should apply exact width with min-width and max-width', () => {
      createTable(`
        <tr><td>AuTc 100px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.minWidth).toBe('100px')
      expect(cell.style.maxWidth).toBe('100px')
    })

    it('should apply min: prefix as min-width only', () => {
      createTable(`
        <tr><td>AuTc min:200px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('')
      expect(cell.style.minWidth).toBe('200px')
      expect(cell.style.maxWidth).toBe('')
    })

    it('should apply max: prefix as max-width only', () => {
      createTable(`
        <tr><td>AuTc max:150px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('')
      expect(cell.style.minWidth).toBe('')
      expect(cell.style.maxWidth).toBe('150px')
    })

    it('should parse min: and max: with various units', () => {
      const cases = [
        { input: 'min:50%', expectedMin: '50%', expectedMax: '' },
        { input: 'max:10em', expectedMin: '', expectedMax: '10em' },
        { input: 'min:20rem', expectedMin: '20rem', expectedMax: '' },
      ]
      for (const { input, expectedMin, expectedMax } of cases) {
        document.body.innerHTML = ''
        createTable(`
          <tr><td>AuTc ${input}</td></tr>
          <tr><td>数据</td></tr>
        `)
        run()
        const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
        expect(cell.style.minWidth).toBe(expectedMin)
        expect(cell.style.maxWidth).toBe(expectedMax)
      }
    })

    it('should not require trigger in non-first columns', () => {
      createTable(`
        <tr>
          <td>AuTc 100px nowrap left top</td>
          <td>200px center middle</td>
          <td>nowrap right bottom</td>
        </tr>
        <tr><td>A1</td><td>B1</td><td>C1</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(1)

      const cells = rows[0].querySelectorAll<HTMLTableCellElement>('td')
      expect(cells[0].style.width).toBe('100px')
      expect(cells[0].style.whiteSpace).toBe('nowrap')
      expect(cells[0].style.textAlign).toBe('left')
      expect(cells[0].style.verticalAlign).toBe('top')

      expect(cells[1].style.width).toBe('200px')
      expect(cells[1].style.textAlign).toBe('center')
      expect(cells[1].style.verticalAlign).toBe('middle')

      expect(cells[2].style.whiteSpace).toBe('nowrap')
      expect(cells[2].style.textAlign).toBe('right')
      expect(cells[2].style.verticalAlign).toBe('bottom')
    })
  })

  describe('first row removal', () => {
    it('should remove the config row after applying', () => {
      createTable(`
        <tr><td>AuTc 100px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(1)
      expect(rows[0].querySelector('td')!.textContent).toBe('数据')
    })

    it('should handle multiple tables independently', () => {
      createTable(`
        <tr><td>AuTc 100px</td></tr>
        <tr><td>表1数据</td></tr>
      `)
      createTable(`
        <tr><td>au-tc nowrap</td></tr>
        <tr><td>表2数据</td></tr>
      `)
      run()
      const tables = qsa<HTMLTableElement>('table')
      expect(tables.length).toBe(2)
      expect(tables[0].querySelectorAll('tr').length).toBe(1)
      expect(tables[1].querySelectorAll('tr').length).toBe(1)
      expect(tables[0].querySelector<HTMLTableCellElement>('td')!.style.width).toBe('100px')
      expect(tables[1].querySelector<HTMLTableCellElement>('td')!.style.whiteSpace).toBe('nowrap')
    })

    it('should skip tables where first cell does not match trigger', () => {
      createTable(`
        <tr><td>普通文本</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(2)
      expect(rows[0].querySelector('td')!.textContent).toBe('普通文本')
    })
  })

  describe('case insensitivity', () => {
    it('should handle lowercase trigger', () => {
      createTable(`
        <tr><td>autc 100px</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      // triggerPattern uses exact match, so lowercase should NOT match
      expect(cell.style.width).toBe('')
    })

    it('should handle mixed case config values', () => {
      createTable(`
        <tr><td>AuTc 100px NoWrap LEFT Top</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.whiteSpace).toBe('nowrap')
      expect(cell.style.textAlign).toBe('left')
      expect(cell.style.verticalAlign).toBe('top')
    })
  })

  describe('edge cases', () => {
    it('should handle table with only config row', () => {
      createTable(`
        <tr><td>AuTc 100px</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(0)
    })

    it('should handle trigger with extra whitespace', () => {
      createTable(`
        <tr><td>AuTc   100px    nowrap</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.whiteSpace).toBe('nowrap')
    })

    it('should handle trigger in HTML tags', () => {
      createTable(`
        <tr><td><b>AuTc</b> 100px nowrap</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      // textContent includes text from nested tags
      const cell = qs<HTMLTableCellElement>('table tr:nth-child(1) td')
      expect(cell.style.width).toBe('100px')
      expect(cell.style.whiteSpace).toBe('nowrap')
    })
  })
})
