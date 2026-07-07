import { describe, it, expect, beforeEach, vi } from 'vitest'

vi.mock('../public/options.json', () => ({
  triggers: ['au-table-float'],
  priority: 1
}))

import { run } from '../lib/auTableFloat'

describe('AuTableFloat', () => {
  beforeEach(() => {
    document.body.innerHTML = ''
  })

  function createWikiViewWithParas(): HTMLElement {
    const wikiView = document.createElement('div')
    wikiView.className = 'wikiView'
    const paras = document.createElement('div')
    paras.className = 'paras'
    wikiView.appendChild(paras)
    document.body.appendChild(wikiView)
    return paras
  }

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
    it('should float table right, add class and move before .paras when trigger is in first cell', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td><td></td></tr>
        <tr><td>数据1</td><td>数据2</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(1)
      expect(rows[0].querySelector('td')!.textContent).toBe('数据1')
      expect(paras.firstElementChild).toBe(table)
    })

    it('should detect trigger in any cell', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>标题</td><td>au-table-float</td></tr>
        <tr><td>数据1</td><td>数据2</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(1)
      expect(rows[0].querySelector('td')!.textContent).toBe('数据1')
      expect(paras.firstElementChild).toBe(table)
    })

    it('should detect trigger wrapped in HTML tags', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td><b>au-table-float</b></td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(1)
      expect(paras.firstElementChild).toBe(table)
    })

    it('should skip tables without trigger cell', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>普通文本</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.classList.contains('au-floated-paras')).toBe(false)
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(2)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should skip partial matches', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float extra</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.classList.contains('au-floated-paras')).toBe(false)
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(2)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should handle trigger with surrounding whitespace', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>  au-table-float  </td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(1)
      expect(paras.firstElementChild).toBe(table)
    })

    it('should show error when no .wikiView .paras exists', () => {
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(1)
      const error = table.nextElementSibling as HTMLElement
      expect(error).not.toBeNull()
      expect(error.tagName).toBe('B')
      expect(error.style.color).toBe('red')
      expect(error.textContent).toBe('找不到.paras元素')
    })

    it('should only use the first .wikiView .paras', () => {
      const firstWikiView = document.createElement('div')
      firstWikiView.className = 'wikiView'
      const firstParas = document.createElement('div')
      firstParas.className = 'paras'
      firstWikiView.appendChild(firstParas)

      const secondWikiView = document.createElement('div')
      secondWikiView.className = 'wikiView'
      const secondParas = document.createElement('div')
      secondParas.className = 'paras'
      secondWikiView.appendChild(secondParas)

      document.body.appendChild(firstWikiView)
      document.body.appendChild(secondWikiView)
      document.body.appendChild(createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据</td></tr>
      `))
      run()
      const table = qs<HTMLTableElement>('table')
      expect(firstParas.firstElementChild).toBe(table)
      expect(secondParas.firstElementChild).toBeNull()
      expect(firstParas.classList.contains('au-floated-paras')).toBe(true)
      expect(secondParas.classList.contains('au-floated-paras')).toBe(false)
    })
  })

  describe('styles injection', () => {
    it('should inject style tag into head on first run', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const style = document.getElementById('au-table-float-styles') as HTMLStyleElement
      expect(style).not.toBeNull()
      expect(style.parentElement).toBe(document.head)
      expect(style.textContent).toContain('.au-floated-table')
      expect(style.textContent).toContain('margin: 10px !important')
      expect(style.textContent).toContain('.au-floated-paras .para')
      expect(style.textContent).toContain('.au-floated-paras .indent')
      expect(style.textContent).toContain('overflow-x: unset')
    })

    it('should not inject duplicate style tags on multiple runs', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      run()
      const styles = document.head.querySelectorAll('#au-table-float-styles')
      expect(styles.length).toBe(1)
    })

    it('should keep style tag at the end of head after multiple runs', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      run()
      const style = document.getElementById('au-table-float-styles')
      expect(style).toBe(document.head.lastElementChild)
    })
  })

  describe('row removal', () => {
    it('should remove multiple trigger rows in one table', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据1</td></tr>
        <tr><td>au-table-float</td></tr>
        <tr><td>数据2</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(2)
      expect(rows[0].querySelector('td')!.textContent).toBe('数据1')
      expect(rows[1].querySelector('td')!.textContent).toBe('数据2')
      expect(paras.firstElementChild).toBe(table)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
    })

    it('should handle table with only trigger row', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(0)
      expect(paras.firstElementChild).toBe(table)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
    })
  })

  describe('multiple tables', () => {
    it('should process multiple tables independently', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>表1数据</td></tr>
      `)
      createTable(`
        <tr><td>普通文本</td></tr>
        <tr><td>表2数据</td></tr>
      `)
      run()
      const tables = qsa<HTMLTableElement>('table')
      expect(tables.length).toBe(2)
      expect(tables[0].style.float).toBe('right')
      expect(tables[0].classList.contains('au-floated-table')).toBe(true)
      expect(tables[1].style.float).toBe('')
      expect(tables[1].classList.contains('au-floated-table')).toBe(false)
      expect(tables[0].querySelectorAll('tr').length).toBe(1)
      expect(tables[1].querySelectorAll('tr').length).toBe(2)
      expect(paras.firstElementChild).toBe(tables[0])
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
    })
  })
})
