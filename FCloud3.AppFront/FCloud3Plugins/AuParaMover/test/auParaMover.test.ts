import { describe, it, expect, beforeEach, vi } from 'vitest'

vi.mock('../public/options.json', () => ({
  triggers: ['AuParaMover', 'au-pm', 'au-para-mover'],
  priority: 0
}))

import { run } from '../lib/auParaMover'

describe('AuParaMover', () => {
  beforeEach(() => {
    document.body.innerHTML = ''
  })

  function createSingleCellTable(text: string): HTMLTableElement {
    const table = document.createElement('table')
    table.innerHTML = `<tr><td>${text}</td></tr>`
    document.body.appendChild(table)
    return table
  }

  describe('target detection', () => {
    it('should detect AuParaMover(param) in single-cell table', () => {
      createSingleCellTable('AuParaMover(简介)')
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      const p = document.createElement('p')
      p.textContent = '这是简介内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      expect(document.querySelector('table')).toBeNull()
      expect(document.querySelector('h1')).toBeNull()
      expect(document.querySelector('p')).not.toBeNull()
    })

    it('should detect alias au-pm(param)', () => {
      createSingleCellTable('au-pm(背景)')
      const h1 = document.createElement('h1')
      h1.textContent = '项目背景'
      const p = document.createElement('p')
      p.textContent = '背景内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      expect(document.querySelector('table')).toBeNull()
      expect(document.querySelector('h1')).toBeNull()
    })

    it('should skip multi-row tables', () => {
      const table = document.createElement('table')
      table.innerHTML = '<tr><td>AuParaMover(简介)</td></tr><tr><td>第二行</td></tr>'
      document.body.appendChild(table)
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      const p = document.createElement('p')
      p.textContent = '内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      expect(document.querySelector('table')).not.toBeNull()
      expect(document.querySelector('h1')).not.toBeNull()
    })

    it('should skip multi-column tables', () => {
      const table = document.createElement('table')
      table.innerHTML = '<tr><td>AuParaMover(简介)</td><td>第二列</td></tr>'
      document.body.appendChild(table)
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      const p = document.createElement('p')
      p.textContent = '内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      expect(document.querySelector('table')).not.toBeNull()
      expect(document.querySelector('h1')).not.toBeNull()
    })

    it('should skip tables without trigger', () => {
      createSingleCellTable('普通文本内容')
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      document.body.appendChild(h1)
      run()
      expect(document.querySelector('table')).not.toBeNull()
      expect(document.querySelector('h1')).not.toBeNull()
    })

    it('should detect trigger inside HTML tags', () => {
      createSingleCellTable('<b>AuParaMover(简介)</b>')
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      const p = document.createElement('p')
      p.textContent = '内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      expect(document.querySelector('table')).toBeNull()
      expect(document.querySelector('h1')).toBeNull()
    })
  })

  describe('paragraph moving', () => {
    it('should move paragraph to table position', () => {
      const section = document.createElement('section')
      section.innerHTML = `
        <h1>项目概述</h1>
        <p>概述内容</p>
        <h1>项目简介</h1>
        <p>简介内容</p>
      `
      document.body.appendChild(section)
      const table = createSingleCellTable('AuParaMover(简介)')
      document.body.insertBefore(table, section)
      run()
      // 表格被删除
      expect(document.querySelector('table')).toBeNull()
      // h1 被删除
      expect(document.querySelector('h1')).not.toBeNull()
      expect(document.querySelectorAll('h1').length).toBe(1)
      // 简介段落被移动到表格位置
      const ps = document.querySelectorAll('p')
      expect(ps.length).toBe(2)
    })

    it('should move multiple paragraphs independently', () => {
      const section = document.createElement('section')
      section.innerHTML = `
        <h1>概述</h1>
        <p>概述内容</p>
        <h1>简介</h1>
        <p>简介内容</p>
      `
      document.body.appendChild(section)
      const table1 = createSingleCellTable('AuParaMover(概述)')
      const table2 = createSingleCellTable('AuParaMover(简介)')
      document.body.insertBefore(table1, section)
      document.body.insertBefore(table2, section)
      run()
      expect(document.querySelectorAll('table').length).toBe(0)
      expect(document.querySelectorAll('h1').length).toBe(0)
      expect(document.querySelectorAll('p').length).toBe(2)
    })

    it('should skip h1 without next sibling', () => {
      // 创建 h1，但后面不跟任何元素
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      document.body.appendChild(h1)
      // 确保 h1 没有 next sibling
      expect(h1.nextElementSibling).toBeNull()

      // 表格放在另一个位置（比如 body 的开头），确保不是 h1 的 next sibling
      const table = document.createElement('table')
      table.innerHTML = '<tr><td>AuParaMover(简介)</td></tr>'
      document.body.insertBefore(table, h1)

      run()
      // h1 没有 next sibling，target 不会被标记为 resolved
      // 最后循环会给 targetCell 设置错误信息，但不会删除表格
      expect(document.querySelector('table')).not.toBeNull()
      expect(document.querySelector('h1')).not.toBeNull()
      const cell = document.querySelector('table td')!
      expect(cell.innerHTML).toContain('未找到包含')
      expect(cell.innerHTML).toContain('color:red')
    })
  })

  describe('error handling', () => {
    it('should show error when no matching h1 found', () => {
      createSingleCellTable('AuParaMover(不存在的标题)')
      const h1 = document.createElement('h1')
      h1.textContent = '完全不同的标题'
      const p = document.createElement('p')
      p.textContent = '内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      expect(document.querySelector('table')).not.toBeNull()
      const cell = document.querySelector('table td')!
      expect(cell.innerHTML).toContain('未找到包含[不存在的标题]的段落标题')
      expect(cell.innerHTML).toContain('color:red')
    })

    it('should show error for unmatched param', () => {
      createSingleCellTable('AuParaMover(不存在的参数)')
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      const p = document.createElement('p')
      p.textContent = '内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      // 参数不匹配任何 h1
      expect(document.querySelector('table')).not.toBeNull()
      const cell = document.querySelector('table td')!
      expect(cell.innerHTML).toContain('未找到包含')
      expect(cell.innerHTML).toContain('color:red')
    })

    it('should resolve only first matching h1 for each target', () => {
      const section = document.createElement('section')
      section.innerHTML = `
        <h1>项目简介</h1>
        <p>第一个简介</p>
        <h1>另一个简介</h1>
        <p>第二个简介</p>
      `
      document.body.appendChild(section)
      createSingleCellTable('AuParaMover(简介)')
      run()
      // 第一个匹配的 h1 被处理（h1 和 p 被移动到表格位置，表格被删除）
      // 第二个 h1 和 p 仍然保留（因为表格已经被删除，没有 target 了）
      // 最终：1 个 h1（第二个）+ 1 个 p（第二个）+ 1 个 p（第一个，被移动）
      expect(document.querySelectorAll('h1').length).toBe(1)
      expect(document.querySelectorAll('p').length).toBe(2)
    })
  })

  describe('DOM manipulation', () => {
    it('should preserve content of moved paragraph', () => {
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      const content = document.createElement('div')
      content.innerHTML = '<p>段落1</p><p>段落2</p>'
      document.body.appendChild(h1)
      document.body.appendChild(content)
      createSingleCellTable('AuParaMover(简介)')
      run()
      expect(document.querySelector('table')).toBeNull()
      expect(document.querySelector('h1')).toBeNull()
      const divs = document.querySelectorAll('div')
      expect(divs.length).toBe(1)
      expect(divs[0].innerHTML).toBe('<p>段落1</p><p>段落2</p>')
    })

    it('should handle trigger inside nested elements', () => {
      createSingleCellTable('<span>AuParaMover(简介)</span>')
      const h1 = document.createElement('h1')
      h1.textContent = '项目简介'
      const p = document.createElement('p')
      p.textContent = '内容'
      document.body.appendChild(h1)
      document.body.appendChild(p)
      run()
      expect(document.querySelector('table')).toBeNull()
      expect(document.querySelector('h1')).toBeNull()
    })
  })
})
