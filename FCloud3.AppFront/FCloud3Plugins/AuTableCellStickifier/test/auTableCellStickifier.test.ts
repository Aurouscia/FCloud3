import { describe, it, expect, beforeEach, vi } from 'vitest'

// 模拟 options.json
vi.mock('../public/options.json', () => ({
  triggers: ['AuTcs', 'au-table-cell-stickifier', 'au-tcs'],
  priority: 1
}))

import { run } from '../lib/auTableCellStickifier'

describe('AuTableCellStickifier', () => {
  beforeEach(() => {
    document.body.innerHTML = ''
  })

  function createTable(html: string): HTMLTableElement {
    const table = document.createElement('table')
    table.innerHTML = html
    document.body.appendChild(table)
    return table
  }

  describe('trigger detection', () => {
    it('should detect AuTcs(right) in first cell', () => {
      createTable(`
        <tr><td>AuTcs(right)</td><td>B</td></tr>
        <tr><td>C</td><td>D</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.hasAttribute('data-autb-sticky-contain')).toBe(true)
      expect(table.rows[0].cells[0].hasAttribute('data-autb-sticky-top')).toBe(true)
      expect(table.rows[0].cells[1].hasAttribute('data-autb-sticky-top')).toBe(true)
    })

    it('should detect AuTcs(below) in first cell', () => {
      createTable(`
        <tr><td>AuTcs(below)</td><td>B</td></tr>
        <tr><td>C</td><td>D</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.hasAttribute('data-autb-sticky-contain')).toBe(true)
      expect(table.rows[0].cells[0].hasAttribute('data-autb-sticky-left')).toBe(true)
      expect(table.rows[1].cells[0].hasAttribute('data-autb-sticky-left')).toBe(true)
    })

    it('should detect AuTcs(right, below)', () => {
      createTable(`
        <tr><td>AuTcs(right, below)</td><td>B</td></tr>
        <tr><td>C</td><td>D</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.hasAttribute('data-autb-sticky-contain')).toBe(true)
      expect(table.rows[0].cells[0].hasAttribute('data-autb-sticky-top')).toBe(true)
      expect(table.rows[0].cells[0].hasAttribute('data-autb-sticky-left')).toBe(true)
    })

    it('should detect alias au-tcs(right)', () => {
      createTable(`
        <tr><td>au-tcs(right)</td><td>B</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.hasAttribute('data-autb-sticky-contain')).toBe(true)
    })

    it('should not process table without trigger', () => {
      createTable(`
        <tr><td>Normal</td><td>B</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.hasAttribute('data-autb-sticky-contain')).toBe(false)
    })
  })

  describe('trigger removal', () => {
    it('should remove trigger text from cell', () => {
      createTable(`
        <tr><td>AuTcs(right)</td><td>B</td></tr>
      `)
      run()
      const cell = document.querySelector('table')!.rows[0].cells[0]
      expect(cell.textContent).toBe('')
    })

    it('should preserve surrounding text', () => {
      createTable(`
        <tr><td>Before AuTcs(right) After</td><td>B</td></tr>
      `)
      run()
      const cell = document.querySelector('table')!.rows[0].cells[0]
      expect(cell.textContent).toContain('Before')
      expect(cell.textContent).toContain('After')
      expect(cell.textContent).not.toContain('AuTcs')
    })

    it('should preserve HTML tags when removing trigger', () => {
      createTable(`
        <tr><td><b>Before</b> AuTcs(right) <i>After</i></td></tr>
      `)
      run()
      const cell = document.querySelector('table')!.rows[0].cells[0]
      expect(cell.textContent).toContain('Before')
      expect(cell.textContent).toContain('After')
      expect(cell.textContent).not.toContain('AuTcs')
      expect(cell.querySelector('b')).not.toBeNull()
      expect(cell.querySelector('i')).not.toBeNull()
    })

    it('should handle trigger inside nested elements', () => {
      createTable(`
        <tr><td><span>AuTcs(below)</span></td></tr>
      `)
      run()
      const cell = document.querySelector('table')!.rows[0].cells[0]
      expect(cell.textContent).toBe('')
      expect(cell.querySelector('span')).not.toBeNull()
    })
  })

  describe('multiple tables', () => {
    it('should process multiple tables independently', () => {
      createTable(`
        <tr><td>AuTcs(right)</td><td>B</td></tr>
      `)
      createTable(`
        <tr><td>AuTcs(below)</td><td>B</td></tr>
      `)
      run()
      const tables = document.querySelectorAll('table')
      expect(tables[0].hasAttribute('data-autb-sticky-contain')).toBe(true)
      expect(tables[1].hasAttribute('data-autb-sticky-contain')).toBe(true)
    })

    it('should skip table without trigger and continue to next', () => {
      createTable(`
        <tr><td>No trigger</td></tr>
      `)
      createTable(`
        <tr><td>AuTcs(right)</td></tr>
      `)
      run()
      const tables = document.querySelectorAll('table')
      expect(tables[0].hasAttribute('data-autb-sticky-contain')).toBe(false)
      expect(tables[1].hasAttribute('data-autb-sticky-contain')).toBe(true)
    })
  })

  describe('search range limit', () => {
    it('should not detect trigger beyond search range (row > 5)', () => {
      createTable(`
        <tr><td>A</td></tr>
        <tr><td>B</td></tr>
        <tr><td>C</td></tr>
        <tr><td>D</td></tr>
        <tr><td>E</td></tr>
        <tr><td>AuTcs(right)</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.hasAttribute('data-autb-sticky-contain')).toBe(false)
    })

    it('should not detect trigger beyond search range (col > 5)', () => {
      createTable(`
        <tr><td>A</td><td>B</td><td>C</td><td>D</td><td>E</td><td>AuTcs(right)</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.hasAttribute('data-autb-sticky-contain')).toBe(false)
    })
  })

  describe('sticky attribute placement', () => {
    it('should set sticky-top from trigger column to row end', () => {
      createTable(`
        <tr><td>A</td><td>AuTcs(right)</td><td>C</td><td>D</td></tr>
      `)
      run()
      const row = document.querySelector('table')!.rows[0]
      expect(row.cells[0].hasAttribute('data-autb-sticky-top')).toBe(false)
      expect(row.cells[1].hasAttribute('data-autb-sticky-top')).toBe(true)
      expect(row.cells[2].hasAttribute('data-autb-sticky-top')).toBe(true)
      expect(row.cells[3].hasAttribute('data-autb-sticky-top')).toBe(true)
    })

    it('should set sticky-left from trigger row to table bottom', () => {
      createTable(`
        <tr><td>A</td><td>B</td></tr>
        <tr><td>AuTcs(below)</td><td>D</td></tr>
        <tr><td>E</td><td>F</td></tr>
      `)
      run()
      const table = document.querySelector('table')!
      expect(table.rows[0].cells[0].hasAttribute('data-autb-sticky-left')).toBe(false)
      expect(table.rows[1].cells[0].hasAttribute('data-autb-sticky-left')).toBe(true)
      expect(table.rows[2].cells[0].hasAttribute('data-autb-sticky-left')).toBe(true)
    })
  })
})
