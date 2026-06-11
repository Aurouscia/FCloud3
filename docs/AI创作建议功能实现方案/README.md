# AI 创作建议功能实现方案

为 FCloud3 添加内置 AI 支持，使用 `Microsoft.Extensions.AI` + OpenAI 兼容的 API 提供商。功能核心是让 AI 为词条编辑提供"创作建议"。AI 可以调用工具获取词条内容、搜索词条，从而给出更精准的建议。

**关键约束**：AI 配置以**团体（UserGroup）**为单位，每个团体可独立设置 API 源和 API Key。

---

## 文档目录

| 文件 | 内容 |
|------|------|
| [01-技术选型.md](./01-技术选型.md) | SDK 选择、NuGet 包 |
| [02-数据库设计.md](./02-数据库设计.md) | 新增实体（AiInstanceConfig、AiConversation、AiMessage、AiUsageRecord）、DbContext 更新 |
| [03-后端架构.md](./03-后端架构.md) | Repo 层、Service 层、Controller 层、服务注册 |
| [04-前端架构.md](./04-前端架构.md) | API 定义、Vue 组件、页面集成 |
| [05-用量统计与计费.md](./05-用量统计与计费.md) | Token 统计、用量查询、限额控制 |
| [06-安全与权限.md](./06-安全与权限.md) | API Key 安全、权限矩阵、速率限制 |
| [07-实现步骤清单.md](./07-实现步骤清单.md) | 分阶段实施清单 |
| [08-注意要点.md](./08-注意要点.md) | DbContext 隔离、流式响应、加密、Token 计数、上下文截断等 |
| [09-参考代码位置.md](./09-参考代码位置.md) | 相关功能现有代码位置速查 |
