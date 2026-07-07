import { describe, it, expect, beforeEach, vi } from 'vitest'

vi.mock('../public/options.json', () => ({
  triggers: ['au-table-float'],
  priority: 1
}))

import { run } from '../lib/auTableFloat'

describe('AuTableFloat', () => {
  beforeEach(() => {
    document.body.innerHTML = ''
    vi.stubGlobal('innerWidth', 1024)
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
    it('should float table right by default on desktop', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td><td></td></tr>
        <tr><td>数据1</td><td>数据2</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(table.style.marginLeft).toBe('10px')
      expect(table.style.marginBottom).toBe('10px')
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

    it('should trigger with extra parameters and default to right', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float extra</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(table.style.marginLeft).toBe('10px')
      expect(table.style.marginBottom).toBe('10px')
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      expect(paras.firstElementChild).toBe(table)
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
      expect(table.style.marginLeft).toBe('10px')
      expect(table.style.marginBottom).toBe('10px')
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

  describe('float direction parameter', () => {
    it('should float left when left is specified', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float left</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('left')
      expect(table.style.marginRight).toBe('10px')
      expect(table.style.marginLeft).toBe('')
      expect(table.style.marginBottom).toBe('10px')
      expect(paras.firstElementChild).toBe(table)
    })

    it('should float right when right is specified', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float right</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.style.marginLeft).toBe('10px')
      expect(table.style.marginRight).toBe('')
      expect(table.style.marginBottom).toBe('10px')
      expect(paras.firstElementChild).toBe(table)
    })

    it('should handle left in mixed case', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float LEFT</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('left')
      expect(table.style.marginRight).toBe('10px')
      expect(table.style.marginBottom).toBe('10px')
      expect(paras.firstElementChild).toBe(table)
    })

    it('should ignore non-direction words', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float something else</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.style.marginLeft).toBe('10px')
      expect(table.style.marginBottom).toBe('10px')
      expect(paras.firstElementChild).toBe(table)
    })
  })

  describe('mobile config', () => {
    it('should default to asis with 500px threshold', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.classList.contains('au-floated-paras')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(1)
    })

    it('should move table on desktop with asis strategy', () => {
      vi.stubGlobal('innerWidth', 800)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(asis, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      expect(paras.firstElementChild).toBe(table)
    })

    it('should not move table on mobile with asis strategy', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(asis, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.classList.contains('au-floated-paras')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should move table regardless of width with enforce strategy', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(enforce, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      expect(paras.firstElementChild).toBe(table)
    })

    it('should move table with drawer strategy on desktop', () => {
      vi.stubGlobal('innerWidth', 800)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
      expect(paras.firstElementChild).toBe(table)
      expect(document.body.querySelectorAll('button').length).toBe(0)
    })

    it('should create drawer with button on mobile', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const button = document.querySelector('button')
      expect(button).not.toBeNull()
      expect(button!.textContent).toBe('打开表格')
      expect(button!.style.display).toBe('block')
      expect(button!.style.margin).toBe('10px auto')

      const backdrop = button!.nextElementSibling as HTMLElement
      expect(backdrop).not.toBeNull()
      expect(backdrop.style.position).toBe('fixed')
      expect(backdrop.style.backgroundColor).toBe('black')
      expect(backdrop.style.opacity).toBe('0')
      expect(backdrop.style.transition).toContain('opacity')
      expect(backdrop.style.zIndex).toBe('9998')

      const panel = backdrop.nextElementSibling as HTMLElement
      expect(panel).not.toBeNull()
      expect(panel.style.position).toBe('fixed')
      expect(panel.style.top).toBe('0px')
      expect(panel.style.bottom).toBe('0px')
      expect(panel.style.backgroundColor).toBe('white')
      expect(panel.style.zIndex).toBe('9999')
      expect(panel.style.transition).toContain('transform')
      expect(panel.classList.contains('au-table-float-drawer')).toBe(true)
      expect(panel.querySelector('table')).not.toBeNull()
    })

    it('should open drawer from right by default when button clicked', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const button = document.querySelector('button')!
      const panel = button.nextElementSibling!.nextElementSibling as HTMLElement
      expect(panel.style.right).toBe('0px')
      expect(panel.style.transform).toBe('translateX(100%)')

      button.click()
      expect(panel.style.transform).toBe('translateX(0)')
      const backdrop = button.nextElementSibling as HTMLElement
      expect(backdrop.style.opacity).toBe('0.4')
      expect(backdrop.style.pointerEvents).toBe('auto')
    })

    it('should open drawer from left when direction is left', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float left mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const button = document.querySelector('button')!
      const panel = button.nextElementSibling!.nextElementSibling as HTMLElement
      expect(panel.style.left).toBe('0px')
      expect(panel.style.transform).toBe('translateX(-100%)')

      button.click()
      expect(panel.style.transform).toBe('translateX(0)')
    })

    it('should close drawer when backdrop clicked', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const button = document.querySelector('button')!
      const backdrop = button.nextElementSibling as HTMLElement
      const panel = backdrop.nextElementSibling as HTMLElement

      button.click()
      expect(panel.style.transform).toBe('translateX(0)')
      expect(backdrop.style.opacity).toBe('0.4')

      backdrop.click()
      expect(panel.style.transform).toBe('translateX(100%)')
      expect(backdrop.style.opacity).toBe('0')
      expect(backdrop.style.pointerEvents).toBe('none')
    })

    it('should use custom button text from third mobile parameter', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px, 查看详情)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const button = document.querySelector('button')
      expect(button).not.toBeNull()
      expect(button!.textContent).toBe('查看详情')
    })

    it('should allow empty second parameter with consecutive commas', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer,,打开)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const button = document.querySelector('button')
      expect(button).not.toBeNull()
      expect(button!.textContent).toBe('打开')
    })

    it('should use custom threshold', () => {
      vi.stubGlobal('innerWidth', 700)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(asis, 800px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should default threshold to 500px when omitted', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(asis)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should default strategy to asis when omitted', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should ignore invalid strategy and use asis', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(invalid, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should ignore invalid threshold and use default', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(asis, invalid)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should allow spaces inside mobile parentheses', () => {
      vi.stubGlobal('innerWidth', 400)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile( asis , 500px )</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('')
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.firstElementChild).toBeNull()
    })

    it('should combine mobile config with direction', () => {
      vi.stubGlobal('innerWidth', 800)
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float left mobile(asis, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('left')
      expect(table.style.marginRight).toBe('10px')
      expect(table.style.marginBottom).toBe('10px')
      expect(paras.firstElementChild).toBe(table)
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
      expect(style.textContent).not.toContain('.au-floated-table')
      expect(style.textContent).not.toContain('margin')
      expect(style.textContent).toContain('.au-floated-paras .para')
      expect(style.textContent).toContain('.au-floated-paras .indent')
      expect(style.textContent).toContain('overflow-x: unset')
      expect(style.textContent).toContain('.au-table-float-drawer')
      expect(style.textContent).toContain('padding: 6px')
      expect(style.textContent).toContain('.au-table-float-drawer table')
      expect(style.textContent).toContain('min-width: 100%')
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
        <tr><td>au-table-float left</td></tr>
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
        <tr><td>au-table-float left mobile(enforce, 500px)</td></tr>
        <tr><td>表1数据</td></tr>
      `)
      createTable(`
        <tr><td>普通文本</td></tr>
        <tr><td>表2数据</td></tr>
      `)
      run()
      const tables = qsa<HTMLTableElement>('table')
      expect(tables.length).toBe(2)
      expect(tables[0].style.float).toBe('left')
      expect(tables[0].classList.contains('au-floated-table')).toBe(true)
      expect(tables[0].style.marginRight).toBe('10px')
      expect(tables[0].style.marginBottom).toBe('10px')
      expect(tables[1].style.float).toBe('')
      expect(tables[1].classList.contains('au-floated-table')).toBe(false)
      expect(tables[0].querySelectorAll('tr').length).toBe(1)
      expect(tables[1].querySelectorAll('tr').length).toBe(2)
      expect(paras.firstElementChild).toBe(tables[0])
      expect(paras.classList.contains('au-floated-paras')).toBe(true)
    })
  })
})
