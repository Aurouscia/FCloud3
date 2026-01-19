# 使用说明
页面上的table标签，若第一行第一列的单元格为约定内容：`AuEChartsCaller(<参数>)`则会被认定为本插件的目标

## 可用参数
（补全这里）

# 项目结构：
- lib
    - AuEChartsCaller.ts 编译入口文件，固定导出一个run函数
- public 
    - trigger.txt 给调用方读取的触发关键词
    - sandbox.html 开发用试验场
- src
    - main.ts 开发用入口文件（开发时查看效果用）