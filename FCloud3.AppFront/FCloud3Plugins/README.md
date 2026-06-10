# 插件目录
这里的插件是在词条加载后按条件触发的js脚本，对内容进行某些更改  

## 编译
在本目录内执行`node buildPlugins.mjs`即可编译所有启用的插件到前端目录内  

## 配置
- 在`pluginsEnabled.mjs`中配置哪些启用，禁用的将其`//`注释掉即可
- 注意：编译后再禁用的插件不会自己消失，需要到下面提到的编译输出目录手动删除
- 前端项目编译时会收集编译好的插件信息(文件名和触发词)，并打包到里面。所以**插件重新编译/配置调整后，前端也需要重新编译**

## 约定
- 插件编译到`../FCloud3Front/public/plugins/<插件名>`目录下
    - 插件的入口：js文件（ecma标准）
        - **名称为 xxx.entry.js**
        - **export一个名为`run`的函数（可同步可异步）**
        - 文件名带哈希，避免客户端不更新
    - 插件的配置：json文件
        - **名称为 options.json**
        - `triggers`：触发词数组，词条内容包含任一触发词时插件才会被加载
        - `priority`：优先级，数值越小运行顺序越靠前，默认为 0
        - 示例：
            ```json
            {
                "triggers": ["AuTimeOffset", "au-time-offset"],
                "priority": 1
            }
            ```

## 开发注意事项

### 正则表达式构造
**不要在模块顶层用模板字符串构造正则**。TypeScript 模板字符串中的 `\\` 在 vitest/bundler 中会被二次转义，导致正则行为异常。正确做法是将正则构建封装到函数内，运行时动态构建：
```typescript
// ❌ 错误：模块顶层
const pattern = `^(?:${triggers.join('|')})$`
const regex = new RegExp(pattern)

// ✅ 正确：函数内运行时构建
function getPattern() {
    return new RegExp(`^(?:${triggers.join('|')})$`)
}
```

### DOM 文本读取
**使用 `textContent` 而非 `innerText`**。jsdom 中 `innerText` 返回 `undefined` 或空字符串，会导致测试失败：
```typescript
// ❌ 错误
const text = cell.innerText.trim()

// ✅ 正确
const text = cell.textContent?.trim() ?? ''
```

### 测试文件类型
测试文件中使用 `querySelector`/`querySelectorAll` 时，返回的 `Element` 类型没有 `style`、`cells` 等属性。应使用泛型参数或类型断言：
```typescript
// ✅ 正确
const cell = document.querySelector<HTMLTableCellElement>('td')!
const cells = document.querySelectorAll<HTMLTableCellElement>('td')
```

### 插件文档
- 在 `public/docs.html` 中编写使用说明
- 在 `public/docs.html` 中引用样式：`<link rel="stylesheet" href="../docs.css">`
- `buildPlugins.mjs` 会自动将 `docs.css` 复制到前端输出目录
- `pluginSearch.cjs` 会自动检测 `docs.html` 并写入 `pluginsFound.json` 的 `docs` 字段

### 公共样式
所有插件文档共享 `FCloud3.AppFront/FCloud3Plugins/docs.css` 中的样式，不要在每个 `docs.html` 中写 `<style>`。

## 许可证
该目录内的子目录，若不另外带LICENSE文件，则默认为使用FCloud3项目的`Apache-2.0`  
若另外使用了其他许可证，则仅在启用时需要遵守，没启用(不编译到前端)时无需遵守