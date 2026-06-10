import { describe, it, expect, beforeEach, vi } from 'vitest'

vi.mock('../lib/targetParser/target', () => ({
  triggers: ['AuTimeOffset', 'au-time-offset', 'au-to']
}))

import { parseTargets, parseTableRow } from '../lib/targetParser/targetParser'

describe('targetParser', () => {
  beforeEach(() => {
    document.body.innerHTML = ''
  })

  function createTable(html: string): HTMLTableElement {
    const table = document.createElement('table')
    table.innerHTML = html
    document.body.appendChild(table)
    return table
  }

  describe('parseTableRow', () => {
    it('should parse date with month/day only', () => {
      const table = createTable('<tr><td>AuTimeOffset(12/25)</td><td>圣诞节</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.specifyTimeOfDay).toBe(false)
        expect(result.desc).toBe('圣诞节')
        expect(result.t.getMonth()).toBe(11) // December
        expect(result.t.getDate()).toBe(25)
      }
    })

    it('should parse date with time', () => {
      const table = createTable('<tr><td>AuTimeOffset(12/25 08:00)</td><td>圣诞早</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.specifyTimeOfDay).toBe(true)
        expect(result.t.getHours()).toBe(8)
        expect(result.t.getMinutes()).toBe(0)
      }
    })

    it('should parse & for current year', () => {
      const table = createTable('<tr><td>AuTimeOffset(&/12/25)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.t.getFullYear()).toBe(new Date().getFullYear())
      }
    })

    it('should parse + for next year', () => {
      const table = createTable('<tr><td>AuTimeOffset(+/12/25)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.t.getFullYear()).toBeGreaterThanOrEqual(new Date().getFullYear())
      }
    })

    it('should parse full year date', () => {
      const table = createTable('<tr><td>AuTimeOffset(2026/6/1)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.t.getFullYear()).toBe(2026)
        expect(result.t.getMonth()).toBe(5)
        expect(result.t.getDate()).toBe(1)
      }
    })

    it('should parse date with dash separator', () => {
      const table = createTable('<tr><td>AuTimeOffset(2026-12-25)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.t.getFullYear()).toBe(2026)
        expect(result.t.getMonth()).toBe(11)
        expect(result.t.getDate()).toBe(25)
      }
    })

    it('should parse time with seconds', () => {
      const table = createTable('<tr><td>AuTimeOffset(12/25 14:30:45)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.specifyTimeOfDay).toBe(true)
        expect(result.t.getHours()).toBe(14)
        expect(result.t.getMinutes()).toBe(30)
        expect(result.t.getSeconds()).toBe(45)
      }
    })

    it('should parse without description', () => {
      const table = createTable('<tr><td>AuTimeOffset(12/25)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.desc).toBeUndefined()
      }
    })

    it('should return false for 0 cells', () => {
      const table = createTable('<tr></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).toBe(false)
    })

    it('should return false for 3 cells', () => {
      const table = createTable('<tr><td>AuTimeOffset(12/25)</td><td>desc</td><td>extra</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).toBe(false)
    })

    it('should return false for invalid date format', () => {
      const table = createTable('<tr><td>AuTimeOffset(invalid)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).toBe(false)
    })

    it('should return false for empty parentheses', () => {
      const table = createTable('<tr><td>AuTimeOffset()</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).toBe(false)
    })

    it('should detect alias au-to', () => {
      const table = createTable('<tr><td>au-to(12/25)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
    })

    it('should detect alias au-time-offset', () => {
      const table = createTable('<tr><td>au-time-offset(12/25)</td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
    })

    it('should parse HTML description', () => {
      const table = createTable('<tr><td>AuTimeOffset(12/25)</td><td><b>圣诞节</b></td></tr>')
      const row = table.rows[0]
      const result = parseTableRow(row)
      expect(result).not.toBe(false)
      if (result !== false) {
        expect(result.desc).toBe('<b>圣诞节</b>')
      }
    })
  })

  describe('parseTargets', () => {
    it('should parse valid table with multiple rows', () => {
      createTable(`
        <tr><td>AuTimeOffset(12/25)</td><td>圣诞节</td></tr>
        <tr><td>AuTimeOffset(1/1)</td><td>元旦</td></tr>
      `)
      const groups = parseTargets()
      expect(groups.length).toBe(1)
      expect(groups[0].targets.length).toBe(2)
      expect(groups[0].targets[0].desc).toBe('圣诞节')
      expect(groups[0].targets[1].desc).toBe('元旦')
    })

    it('should skip table with invalid row', () => {
      createTable(`
        <tr><td>AuTimeOffset(12/25)</td><td>圣诞节</td></tr>
        <tr><td>invalid row</td></tr>
      `)
      const groups = parseTargets()
      expect(groups.length).toBe(0)
    })

    it('should parse multiple tables independently', () => {
      createTable(`
        <tr><td>AuTimeOffset(12/25)</td><td>圣诞节</td></tr>
      `)
      createTable(`
        <tr><td>AuTimeOffset(1/1)</td><td>元旦</td></tr>
      `)
      const groups = parseTargets()
      expect(groups.length).toBe(2)
      expect(groups[0].targets[0].desc).toBe('圣诞节')
      expect(groups[1].targets[0].desc).toBe('元旦')
    })

    it('should skip table without trigger', () => {
      createTable(`
        <tr><td>普通文本</td></tr>
      `)
      const groups = parseTargets()
      expect(groups.length).toBe(0)
    })

    it('should skip empty table', () => {
      createTable('')
      const groups = parseTargets()
      expect(groups.length).toBe(0)
    })
  })
})
