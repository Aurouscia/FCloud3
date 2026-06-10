import { describe, it, expect, beforeEach, vi } from 'vitest'

vi.mock('../public/options.json', () => ({
  triggers: ['AuParaLoader', 'au-pl', 'au-para-loader'],
  priority: 0
}))

import { run } from '../lib/auParaLoader'

describe('AuParaLoader', () => {
  beforeEach(() => {
    document.body.innerHTML = ''
    vi.restoreAllMocks()
  })

  function createSingleCellTable(text: string): HTMLTableElement {
    const table = document.createElement('table')
    table.innerHTML = `<tr><td>${text}</td></tr>`
    document.body.appendChild(table)
    return table
  }

  function mockFetchResponse(paras: Array<{ Title: string; Content: string }>) {
    globalThis.fetch = vi.fn().mockResolvedValue({
      json: async () => ({ Paras: paras })
    } as unknown as Response)
  }

  describe('target detection', () => {
    it('should detect AuParaLoader(pathName) in single-cell table', async () => {
      createSingleCellTable('AuParaLoader(TestWiki)')
      mockFetchResponse([{ Title: '默认段落', Content: '<p>加载的内容</p>' }])
      await run()
      expect(document.querySelector('div[loaded-from]')).not.toBeNull()
      expect(document.querySelector('div[loaded-from]')!.innerHTML).toBe('<p>加载的内容</p>')
    })

    it('should detect alias au-pl(pathName)', async () => {
      createSingleCellTable('au-pl(TestWiki)')
      mockFetchResponse([{ Title: '默认段落', Content: '<p>别名加载</p>' }])
      await run()
      expect(document.querySelector('div[loaded-from]')).not.toBeNull()
    })

    it('should detect AuParaLoader(pathName, paraSelect, urlBase)', async () => {
      createSingleCellTable('AuParaLoader(TestWiki, 简介, http://example.com)')
      mockFetchResponse([
        { Title: '其他', Content: '<p>不匹配</p>' },
        { Title: '项目简介', Content: '<p>简介内容</p>' }
      ])
      await run()
      const div = document.querySelector('div[loaded-from]')
      expect(div).not.toBeNull()
      expect(div!.innerHTML).toBe('<p>简介内容</p>')
      expect(div!.getAttribute('loaded-from')).toBe('http://example.com/api/WikiParsing/GetParsedWiki?pathName=TestWiki')
    })

    it('should skip multi-row tables', async () => {
      const table = document.createElement('table')
      table.innerHTML = '<tr><td>AuParaLoader(TestWiki)</td></tr><tr><td>第二行</td></tr>'
      document.body.appendChild(table)
      mockFetchResponse([{ Title: '默认段落', Content: '<p>内容</p>' }])
      await run()
      expect(document.querySelector('div[loaded-from]')).toBeNull()
    })

    it('should skip multi-column tables', async () => {
      const table = document.createElement('table')
      table.innerHTML = '<tr><td>AuParaLoader(TestWiki)</td><td>第二列</td></tr>'
      document.body.appendChild(table)
      mockFetchResponse([{ Title: '默认段落', Content: '<p>内容</p>' }])
      await run()
      expect(document.querySelector('div[loaded-from]')).toBeNull()
    })

    it('should skip tables without trigger', async () => {
      createSingleCellTable('普通文本内容')
      mockFetchResponse([{ Title: '默认段落', Content: '<p>内容</p>' }])
      await run()
      expect(document.querySelector('div[loaded-from]')).toBeNull()
    })

    it('should detect trigger inside HTML tags', async () => {
      createSingleCellTable('<b>AuParaLoader(TestWiki)</b>')
      mockFetchResponse([{ Title: '默认段落', Content: '<p>内容</p>' }])
      await run()
      expect(document.querySelector('div[loaded-from]')).not.toBeNull()
    })
  })

  describe('paragraph selection', () => {
    it('should select first paragraph when paraSelect is empty', async () => {
      createSingleCellTable('AuParaLoader(TestWiki,)')
      mockFetchResponse([
        { Title: '第一段', Content: '<p>第一段内容</p>' },
        { Title: '第二段', Content: '<p>第二段内容</p>' }
      ])
      await run()
      const div = document.querySelector('div[loaded-from]')
      expect(div!.innerHTML).toBe('<p>第一段内容</p>')
    })

    it('should find paragraph by partial title match', async () => {
      createSingleCellTable('AuParaLoader(TestWiki, 简介)')
      mockFetchResponse([
        { Title: '项目概述', Content: '<p>概述内容</p>' },
        { Title: '项目简介与背景', Content: '<p>简介内容</p>' }
      ])
      await run()
      const div = document.querySelector('div[loaded-from]')
      expect(div!.innerHTML).toBe('<p>简介内容</p>')
    })

    it('should show error when paragraph not found', async () => {
      createSingleCellTable('AuParaLoader(TestWiki, 不存在的段落)')
      mockFetchResponse([
        { Title: '第一段', Content: '<p>内容</p>' }
      ])
      await run()
      expect(document.querySelector('div[loaded-from]')).toBeNull()
      const cell = document.querySelector('table td')!
      expect(cell.innerHTML).toContain('找不到指定段落')
      expect(cell.innerHTML).toContain('color:red')
    })
  })

  describe('error handling', () => {
    it('should show error when pathName is missing', async () => {
      createSingleCellTable('AuParaLoader(  )')
      await run()
      const cell = document.querySelector('table td')!
      expect(cell.innerHTML).toContain('缺少词条路径名')
      expect(cell.innerHTML).toContain('color:red')
    })

    it('should show error on network failure', async () => {
      createSingleCellTable('AuParaLoader(TestWiki)')
      globalThis.fetch = vi.fn().mockRejectedValue(new Error('Network error'))
      await run()
      const cell = document.querySelector('table td')!
      expect(cell.innerHTML).toContain('网络请求失败')
      expect(cell.innerHTML).toContain('color:red')
    })

    it('should show error on invalid JSON response', async () => {
      createSingleCellTable('AuParaLoader(TestWiki)')
      globalThis.fetch = vi.fn().mockResolvedValue({
        json: async () => { throw new Error('Invalid JSON') }
      } as unknown as Response)
      await run()
      const cell = document.querySelector('table td')!
      expect(cell.innerHTML).toContain('http异常')
      expect(cell.innerHTML).toContain('color:red')
    })

    it('should continue processing next target after one fails', async () => {
      createSingleCellTable('AuParaLoader(  )')
      createSingleCellTable('AuParaLoader(TestWiki)')
      globalThis.fetch = vi.fn().mockResolvedValue({
        json: async () => ({ Paras: [{ Title: '默认段落', Content: '<p>成功</p>' }] })
      } as unknown as Response)
      await run()
      const tables = document.querySelectorAll('table')
      // 第一个表格显示错误（仍然保留为table）
      expect(tables[0].querySelector('td')!.innerHTML).toContain('缺少词条路径名')
      // 第二个表格被替换为div
      expect(document.querySelectorAll('div[loaded-from]').length).toBe(1)
    })

    it('should continue after paragraph not found', async () => {
      createSingleCellTable('AuParaLoader(WikiA, 不存在)')
      createSingleCellTable('AuParaLoader(WikiB)')
      globalThis.fetch = vi.fn().mockResolvedValue({
        json: async () => ({ Paras: [{ Title: '默认段落', Content: '<p>成功</p>' }] })
      } as unknown as Response)
      await run()
      const tables = document.querySelectorAll('table')
      expect(tables[0].querySelector('td')!.innerHTML).toContain('找不到指定段落')
      expect(document.querySelectorAll('div[loaded-from]').length).toBe(1)
    })
  })

  describe('DOM replacement', () => {
    it('should replace table with div containing loaded content', async () => {
      createSingleCellTable('AuParaLoader(TestWiki)')
      mockFetchResponse([{ Title: '默认段落', Content: '<h2>标题</h2><p>正文</p>' }])
      await run()
      expect(document.querySelector('table')).toBeNull()
      const div = document.querySelector('div[loaded-from]')
      expect(div).not.toBeNull()
      expect(div!.innerHTML).toBe('<h2>标题</h2><p>正文</p>')
    })

    it('should set loaded-from attribute with correct URL', async () => {
      createSingleCellTable('AuParaLoader(TestWiki, , http://wiki.example.com)')
      mockFetchResponse([{ Title: '默认段落', Content: '<p>内容</p>' }])
      await run()
      const div = document.querySelector('div[loaded-from]')
      expect(div!.getAttribute('loaded-from')).toBe('http://wiki.example.com/api/WikiParsing/GetParsedWiki?pathName=TestWiki')
    })

    it('should use default URL when urlBase is not provided', async () => {
      createSingleCellTable('AuParaLoader(TestWiki)')
      mockFetchResponse([{ Title: '默认段落', Content: '<p>内容</p>' }])
      await run()
      const div = document.querySelector('div[loaded-from]')
      expect(div!.getAttribute('loaded-from')).toBe('/api/WikiParsing/GetParsedWiki?pathName=TestWiki')
    })
  })

  describe('multiple targets', () => {
    it('should process multiple tables independently', async () => {
      createSingleCellTable('AuParaLoader(WikiA)')
      createSingleCellTable('AuParaLoader(WikiB)')
      globalThis.fetch = vi.fn().mockImplementation((url: string) => {
        const pathName = url.includes('WikiA') ? 'WikiA' : 'WikiB'
        return Promise.resolve({
          json: async () => ({ Paras: [{ Title: '默认段落', Content: `<p>${pathName}内容</p>` }] })
        } as unknown as Response)
      })
      await run()
      const divs = document.querySelectorAll('div[loaded-from]')
      expect(divs.length).toBe(2)
    })
  })
})
