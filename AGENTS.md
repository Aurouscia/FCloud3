# FCloud3

本项目为一个介于论坛和 wiki 之间的系统，允许用户编辑内容（词条），以及相关的各种设置

本项目架构
- 后端 asp.net Core (.net10) 
- 前端 vue3 (TypeScript + Vite + Sass)

## 后端

- FCloud3.App 应用项目
- FCloud3.DbContexts 数据库上下文项目
- FCloud3.Diff 词条差异生成器项目
- FCloud3.Entities 实体类项目
- FCloud3.Repos repository 项目
    - 特殊情况除外，所有类都应继承 RepoBase&lt;T&gt;，并以 XxxRepo 命名
- FCloud3.Services 服务类项目
    - 所有类都应以 XxxService 命名
- FCloud3.WikiPreprocessor 词条解析器（本软件语法转为html）

### 注意事项

- 尽量使用新版 C# 语法（主构造函数，集合表达式）
- 改完后通过 dotnet build 验证编译是否通过

## 前端

- src/models 定义接口传输的数据结构（目录结构与后端 FCloud3.App/Controllers 大致保持一致，应 ls/dir 确认）
- src/pages 所有页面（目录结构同上，应 ls/dir 确认）
    - 怎么注册路由：参考 src\pages\Diff\routes 目录的内容
- src\utils\com\api.ts 定义所有的 api 调用代码，是一个对象，其对象结构和 Controllers 保持一致，且返回值通过 as + src/models 中定义的类型确定
    - 如果需要为其添加请求，先读取 src\utils\com\httpClient.ts，了解 request 函数用法
- import { injectApi, injectPop } from '@/provides';
    - 在页面中使用 const api = injectApi() 获取上述的 api 对象，进行数据交互
    - 在页面中使用 const pop = injectPop() 获取“信息显示”组件的引用
        - pop.value.show("不能提交空评论", "failed"); "success"|"failed"|"warning"|"info"

### 注意事项

- 除非迫不得已，否则不得使用 any 和 as，使用后必须向用户报告
- 改完后通过 npm run build 验证编译是否通过