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

  function resizeTo(width: number) {
    vi.stubGlobal('innerWidth', width)
    window.dispatchEvent(new Event('resize'))
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
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(paras.classList.contains('au-floated-paras')).toBe(false)
      expect(qsa<HTMLTableRowElement>('table tr').length).toBe(2)
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
      const error = table.nextElementSibling as HTMLElement
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
    })
  })

  describe('float direction parameter', () => {
    it('should float left when left is specified', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float left</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('left')
      expect(table.style.marginRight).toBe('10px')
      expect(table.style.marginLeft).toBe('')
    })

    it('should float right when right is specified', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float right</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('right')
    })
  })

  describe('asis responsive switching', () => {
    it('should move to float on desktop and back to original on mobile', () => {
      const paras = createWikiViewWithParas()
      const table = createTable(`
        <tr><td>au-table-float mobile(asis, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      const originalParent = table.parentElement
      run()
      expect(paras.firstElementChild).toBe(table)
      expect(table.classList.contains('au-floated-table')).toBe(true)

      resizeTo(400)
      expect(table.parentElement).toBe(originalParent)
      expect(table.classList.contains('au-floated-table')).toBe(false)
      expect(table.style.float).toBe('')

      resizeTo(1024)
      expect(paras.firstElementChild).toBe(table)
      expect(table.classList.contains('au-floated-table')).toBe(true)
      expect(table.style.float).toBe('right')
    })
  })

  describe('drawer responsive switching', () => {
    it('should move to float on desktop and create drawer on mobile', () => {
      const paras = createWikiViewWithParas()
      const table = createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      const originalParent = table.parentElement
      run()
      expect(paras.firstElementChild).toBe(table)
      expect(document.querySelector('button')).toBeNull()

      resizeTo(400)
      expect(table.parentElement).not.toBe(originalParent)
      expect(table.parentElement).not.toBe(paras)
      const button = document.querySelector('button')
      expect(button).not.toBeNull()
      expect(button!.textContent).toBe('打开表格')
      expect(button!.parentElement).toBe(originalParent)
      const panel = document.querySelector('.au-table-float-panel') as HTMLElement
      expect(panel).not.toBeNull()
      expect(panel.contains(table)).toBe(true)

      resizeTo(1024)
      expect(paras.firstElementChild).toBe(table)
      expect(document.querySelector('button')).toBeNull()
      expect(document.querySelector('.au-table-float-backdrop')).toBeNull()
      expect(document.querySelector('.au-table-float-panel')).toBeNull()
    })

    it('should remove drawer elements when switching to desktop', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      expect(document.querySelector('button')).not.toBeNull()

      resizeTo(1024)
      expect(document.querySelector('button')).toBeNull()
      expect(document.querySelector('.au-table-float-backdrop')).toBeNull()
      expect(document.querySelector('.au-table-float-panel')).toBeNull()
    })

    it('should open and close drawer', () => {
      vi.stubGlobal('innerWidth', 400)
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const button = document.querySelector('button')!
      const backdrop = document.querySelector('.au-table-float-backdrop') as HTMLElement
      const panel = document.querySelector('.au-table-float-panel') as HTMLElement
      expect(panel.style.transform).toBe('translateX(100%)')

      button.click()
      expect(panel.style.transform).toBe('translateX(0)')
      expect(backdrop.style.opacity).toBe('0.4')

      backdrop.click()
      expect(panel.style.transform).toBe('translateX(100%)')
      expect(backdrop.style.opacity).toBe('0')
    })

    it('should use custom button text and support empty threshold', () => {
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

    it('should recreate button after opening drawer and resizing to desktop then mobile', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(drawer, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      expect(paras.contains(qs('table'))).toBe(true)

      resizeTo(400)
      const button1 = document.querySelector('button') as HTMLButtonElement
      expect(button1).not.toBeNull()
      button1.click()
      expect(document.querySelector('.au-table-float-panel')!.classList.contains('au-table-float-panel')).toBe(true)

      resizeTo(1024)
      expect(paras.contains(qs('table'))).toBe(true)
      expect(document.querySelector('button')).toBeNull()
      expect(document.querySelector('.au-table-float-backdrop')).toBeNull()
      expect(document.querySelector('.au-table-float-panel')).toBeNull()

      resizeTo(400)
      const button2 = document.querySelector('button') as HTMLButtonElement
      expect(button2).not.toBeNull()
      expect(button2.textContent).toBe('打开表格')
      expect(document.querySelector('.au-table-float-panel')?.contains(qs('table'))).toBe(true)
    })
  })

  describe('enforce strategy', () => {
    it('should always float regardless of width changes', () => {
      const paras = createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float mobile(enforce, 500px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      expect(paras.firstElementChild).toBeTruthy()

      resizeTo(400)
      expect(paras.firstElementChild).toBe(qs('table'))
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
      expect(style.textContent).toContain('.au-table-float-drawer')
      expect(style.textContent).toContain('padding: 6px')
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
  })

  describe('row removal', () => {
    it('should remove multiple trigger rows in one table', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float</td></tr>
        <tr><td>数据1</td></tr>
        <tr><td>au-table-float left</td></tr>
        <tr><td>数据2</td></tr>
      `)
      run()
      const rows = qsa<HTMLTableRowElement>('table tr')
      expect(rows.length).toBe(2)
    })
  })

  describe('css style parameters', () => {
    it('should apply font-size to table style', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float font-size(14px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.fontSize).toBe('14px')
    })

    it('should apply width to table style', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float width(50%)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.width).toBe('50%')
    })

    it('should apply max-width and min-width to table style', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float max-width(300px) min-width(100px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.maxWidth).toBe('300px')
      expect(table.style.minWidth).toBe('100px')
    })

    it('should apply multiple css styles together with direction and mobile', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float left mobile(enforce) font-size(12px) width(200px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.float).toBe('left')
      expect(table.style.fontSize).toBe('12px')
      expect(table.style.width).toBe('200px')
    })

    it('should ignore empty css style values', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float font-size()</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.fontSize).toBe('')
    })

    it('should be case-insensitive for css property names', () => {
      createWikiViewWithParas()
      createTable(`
        <tr><td>au-table-float FONT-SIZE(16px) Max-Width(400px)</td></tr>
        <tr><td>数据</td></tr>
      `)
      run()
      const table = qs<HTMLTableElement>('table')
      expect(table.style.fontSize).toBe('16px')
      expect(table.style.maxWidth).toBe('400px')
    })
  })

  describe('multiple tables with different thresholds', () => {
    it('should handle each table according to its own threshold', () => {
      const paras = createWikiViewWithParas()
      const tableA = createTable(`
        <tr><td>au-table-float mobile(asis, 800px)</td></tr>
        <tr><td>A</td></tr>
      `)
      const tableB = createTable(`
        <tr><td>au-table-float mobile(drawer, 600px)</td></tr>
        <tr><td>B</td></tr>
      `)
      const originalParentA = tableA.parentElement
      const originalParentB = tableB.parentElement

      run()
      expect(paras.contains(tableA)).toBe(true)
      expect(paras.contains(tableB)).toBe(true)
      expect(document.querySelectorAll('button').length).toBe(0)

      // width 700: A is mobile (700 < 800), B is desktop (700 >= 600)
      resizeTo(700)
      expect(tableA.parentElement).toBe(originalParentA)
      expect(paras.contains(tableB)).toBe(true)
      expect(document.querySelectorAll('button').length).toBe(0)

      // width 500: A is mobile, B is mobile (500 < 600)
      resizeTo(500)
      expect(tableA.parentElement).toBe(originalParentA)
      expect(tableB.parentElement).not.toBe(originalParentB)
      expect(tableB.parentElement).not.toBe(paras)
      expect(document.querySelectorAll('button').length).toBe(1)

      // width 1024: both desktop
      resizeTo(1024)
      expect(paras.contains(tableA)).toBe(true)
      expect(paras.contains(tableB)).toBe(true)
      expect(document.querySelectorAll('button').length).toBe(0)
    })
  })
})
