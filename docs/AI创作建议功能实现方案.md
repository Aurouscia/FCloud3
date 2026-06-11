# AI 创作建议功能实现方案

## 概述

为 FCloud3 添加内置 AI 支持，使用 `Microsoft.Extensions.AI` + OpenAI 兼容的 API 提供商。功能核心是让 AI 为词条编辑提供"创作建议"。AI 可以调用工具获取词条内容、搜索词条，从而给出更精准的建议。

**关键约束**：AI 配置以**团体（UserGroup）**为单位，每个团体可独立设置 API 源和 API Key。

---

## 一、技术选型

| 组件 | 选择 | 说明 |
|------|------|------|
| AI SDK | `Microsoft.Extensions.AI` (MEAI) | 微软官方抽象层，支持 OpenAI 兼容协议 |
| OpenAI 客户端 | `Microsoft.Extensions.AI.OpenAI` | 通过 OpenAI 兼容协议连接任意提供商 |
| 前端 AI 对话界面 | 自建 Vue 组件 | 简洁的对话式 UI，集成在词条编辑页面 |

### 需要添加的 NuGet 包

在 `FCloud3.App.csproj` 中添加：

```xml
<PackageReference Include="Microsoft.Extensions.AI" Version="9.4.0" />
<PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.4.0" />
```

> 版本号以实际最新稳定版为准。MEAI 9.x 支持 .NET 10。

---

## 二、数据库设计

### 2.1 新增实体：`GroupAiConfig`

存储每个团体的 AI 配置（API 源、Key、模型名、系统提示词等）。

**文件**：`FCloud3.Entities/Identities/GroupAiConfig.cs`

```csharp
namespace FCloud3.Entities.Identities
{
    public class GroupAiConfig : IDbModel
    {
        public int Id { get; set; }
        /// <summary>关联的团体Id</summary>
        public int GroupId { get; set; }
        /// <summary>API 基础地址，如 https://api.openai.com/v1</summary>
        public string? ApiBaseUrl { get; set; }
        /// <summary>API Key（建议加密存储）</summary>
        public string? ApiKey { get; set; }
        /// <summary>模型名称，如 gpt-4o</summary>
        public string? ModelName { get; set; }
        /// <summary>系统提示词</summary>
        public string? SystemPrompt { get; set; }
        /// <summary>是否启用 AI 功能</summary>
        public bool Enabled { get; set; }
        /// <summary>默认 AI 可查看的目录范围（某目录及其子级），0 表示不限</summary>
        public int DefaultDirId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
```

### 2.2 新增实体：`AiConversation`

存储用户创建的 AI 对话会话。

**文件**：`FCloud3.Entities/Ai/AiConversation.cs`

```csharp
namespace FCloud3.Entities.Ai
{
    public class AiConversation : IDbModel
    {
        public int Id { get; set; }
        /// <summary>创建者用户Id</summary>
        public int UserId { get; set; }
        /// <summary>关联的团体Id</summary>
        public int GroupId { get; set; }
        /// <summary>对话标题（用户可编辑，默认取第一条用户消息前20字）</summary>
        public string? Title { get; set; }
        /// <summary>创建时关联的词条路径名（可为空）</summary>
        public string? CurrentWikiPathName { get; set; }
        /// <summary>消息数缓存</summary>
        public int MessageCount { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int titleMaxLength = 64;
    }
}
```

### 2.3 新增实体：`AiMessage`

存储对话中的每条消息。

**文件**：`FCloud3.Entities/Ai/AiMessage.cs`

```csharp
namespace FCloud3.Entities.Ai
{
    public class AiMessage : IDbModel
    {
        public int Id { get; set; }
        /// <summary>所属对话Id</summary>
        public int ConversationId { get; set; }
        /// <summary>消息角色</summary>
        public AiMessageRole Role { get; set; }
        /// <summary>消息内容</summary>
        public string? Content { get; set; }
        /// <summary>AI 调用的工具记录（JSON 序列化，可为空）</summary>
        public string? ToolCalls { get; set; }
        /// <summary>消息在对话中的顺序</summary>
        public int Order { get; set; }
        /// <summary>本条消息的 Token 数（用于上下文截断和用量统计）</summary>
        public int TokenCount { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }

    public enum AiMessageRole
    {
        System = 0,
        User = 1,
        Assistant = 2,
        Tool = 3
    }
}
```

### 2.4 更新 `FCloudContext`

在 `FCloud3.DbContexts/FCloudContext.cs` 中添加：

```csharp
public DbSet<GroupAiConfig> GroupAiConfigs { get; set; }
public DbSet<AiConversation> AiConversations { get; set; }
public DbSet<AiMessage> AiMessages { get; set; }
```

### 2.5 迁移注意事项

- **如果是二次开发且需维持 upstream**：必须在**单独的 DbContext** 中创建此实体，使用**分离的迁移目录**，不要修改原有的 `FCloudContext` 和迁移。
- 如果不确定是否维持 upstream，请先询问用户确认。

---

## 三、后端架构

### 3.1 目录结构（新增文件）

```
FCloud3.Repos/
  Identities/
    GroupAiConfigRepo.cs          # 数据访问
  Ai/
    AiConversationRepo.cs         # 对话会话数据访问
    AiMessageRepo.cs              # 对话消息数据访问

FCloud3.Services/
  Ai/
    AiChatService.cs              # AI 对话核心服务
    AiToolService.cs              # AI 工具（获取词条、搜索词条）
    GroupAiConfigService.cs       # 团体 AI 配置管理

FCloud3.App/
  Controllers/
    Ai/
      AiChatController.cs         # AI 对话接口（流式/非流式）+ 对话管理
    Identities/
      GroupAiConfigController.cs  # 团体 AI 配置 CRUD
```

### 3.2 Repo 层：`GroupAiConfigRepo`

继承 `RepoBase<GroupAiConfig>`，遵循项目规范。

```csharp
namespace FCloud3.Repos.Identities
{
    public class GroupAiConfigRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<GroupAiConfig>(context, userIdProvider)
    {
        public GroupAiConfig? GetByGroupId(int groupId)
            => Existing.FirstOrDefault(x => x.GroupId == groupId);
    }
}
```

### 3.3 Repo 层：`AiConversationRepo` 和 `AiMessageRepo`

```csharp
namespace FCloud3.Repos.Ai
{
    public class AiConversationRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiConversation>(context, userIdProvider)
    {
        public List<AiConversation> GetByUserAndGroup(int userId, int groupId)
            => Existing.Where(x => x.UserId == userId && x.GroupId == groupId)
                       .OrderByDescending(x => x.Updated)
                       .ToList();
    }

    public class AiMessageRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiMessage>(context, userIdProvider)
    {
        public List<AiMessage> GetByConversationId(int conversationId)
            => Existing.Where(x => x.ConversationId == conversationId)
                       .OrderBy(x => x.Order)
                       .ToList();

        public int GetMaxOrder(int conversationId)
            => Existing.Where(x => x.ConversationId == conversationId)
                       .Max(x => (int?)x.Order) ?? 0;
    }
}
```

### 3.4 Service 层

#### 3.4.1 `GroupAiConfigService`

管理团体 AI 配置的增删改查，权限检查（仅团体所有者/管理员可修改）。

```csharp
public class GroupAiConfigService(
    GroupAiConfigRepo repo,
    UserGroupRepo userGroupRepo,
    IOperatingUserIdProvider userIdProvider)
{
    public GroupAiConfig? GetConfig(int groupId, out string? errmsg)
    {
        // 检查当前用户是否为该团体成员
        errmsg = null;
        return repo.GetByGroupId(groupId);
    }

    public bool SetConfig(int groupId, string apiBaseUrl, string apiKey, string modelName,
        string? systemPrompt, bool enabled, out string? errmsg)
    {
        // 权限检查：仅团体所有者或管理员可设置
        // ...
    }
}
```

#### 3.3.2 `AiToolService`

提供 AI 可调用的工具方法：

| 工具名 | 功能 | 依赖 |
|--------|------|------|
| `GetWikiContent` | 根据路径名获取词条原始文本内容 | `WikiParsingService`, `TextSectionRepo` |
| `SearchWiki` | 根据关键词搜索词条 | `WikiItemRepo`, `WikiContentSearchService` 或 `QuickSearchService` |

```csharp
public class AiToolService(
    WikiParsingService wikiParsingService,
    WikiItemRepo wikiItemRepo,
    QuickSearchService quickSearchService)
{
    /// <summary>获取指定词条的纯文本内容（用于 AI 分析）</summary>
    public string? GetWikiContent(string pathName)
    {
        var result = wikiParsingService.GetParsedWiki(pathName);
        if (result.Id == 0) return null;
        // 提取所有文本段落的纯文本
        var texts = result.Paras
            .Where(p => p.ParaType == WikiParaType.Text)
            .Select(p => $"## {p.Title}\n{p.Content}")
            .ToList();
        return string.Join("\n\n", texts);
    }

    /// <summary>根据关键词搜索词条</summary>
    public List<WikiSearchResult> SearchWiki(string keyword)
    {
        var q = wikiItemRepo.QuickSearch(keyword, false, 0);
        return q.Take(5).Select(x => new WikiSearchResult(x.Title, x.UrlPathName)).ToList();
    }
}

public record WikiSearchResult(string? Title, string? UrlPathName);
```

#### 3.4.3 `AiChatService`

核心 AI 对话服务，使用 `Microsoft.Extensions.AI`，支持对话历史持久化。

```csharp
using Microsoft.Extensions.AI;

public class AiChatService(
    GroupAiConfigService configService,
    AiToolService toolService,
    AiConversationRepo conversationRepo,
    AiMessageRepo messageRepo,
    AiUsageRecorder usageRecorder,
    IOperatingUserIdProvider userIdProvider)
{
    /// <summary>
    /// 获取 AI 建议。conversationId 为 null 时不保存历史（无状态模式）。
    /// </summary>
    public async IAsyncEnumerable<string> GetSuggestions(
        int groupId, string userPrompt, int? conversationId,
        string? currentWikiPathName, [EnumeratorCancellation] CancellationToken ct)
    {
        var config = configService.GetConfig(groupId, out var errmsg);
        if (config is null || !config.Enabled || string.IsNullOrEmpty(config.ApiKey))
        {
            yield return "AI 功能未启用或配置不完整";
            yield break;
        }

        // 构建 OpenAI 客户端
        var client = new OpenAIClient(
            new ApiKeyCredential(config.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri(config.ApiBaseUrl!) });

        var chatClient = client.AsChatClient(config.ModelName!);

        // 构建消息列表
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, config.SystemPrompt ?? "你是一个 wiki 创作助手...")
        };

        // 加载对话历史（如果有）
        List<AiMessage> history = [];
        if (conversationId.HasValue)
        {
            history = LoadHistoryMessages(conversationId.Value, config.MaxContextMessages);
            foreach (var h in history)
            {
                messages.Add(ConvertToChatMessage(h));
            }
        }

        // 如果有当前词条编辑上下文，注入词条内容
        if (!string.IsNullOrEmpty(currentWikiPathName))
        {
            var content = toolService.GetWikiContent(currentWikiPathName);
            if (content is not null)
            {
                messages.Add(new ChatMessage(ChatRole.User,
                    $"当前正在编辑的词条内容：\n```\n{content}\n```"));
            }
        }

        messages.Add(new ChatMessage(ChatRole.User, userPrompt));

        // 定义工具
        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(toolService.GetWikiContent, name: "get_wiki_content"),
            AIFunctionFactory.Create(toolService.SearchWiki, name: "search_wiki")
        };

        // 流式调用并收集完整回复
        string fullResponse = "";
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = tools }, ct))
        {
            fullResponse += update.Text ?? "";
            yield return update.Text ?? "";
        }

        // 保存消息到数据库（如果有 conversationId）
        if (conversationId.HasValue)
        {
            SaveMessages(conversationId.Value, userPrompt, fullResponse, history.Count);
        }
    }

    /// <summary>创建新对话</summary>
    public AiConversation CreateConversation(int userId, int groupId,
        string? title, string? currentWikiPathName)
    {
        var conversation = new AiConversation
        {
            UserId = userId,
            GroupId = groupId,
            Title = title,
            CurrentWikiPathName = currentWikiPathName,
            MessageCount = 0
        };
        conversationRepo.Add(conversation);
        return conversation;
    }

    /// <summary>获取用户的对话列表</summary>
    public List<AiConversation> GetConversations(int userId, int groupId)
        => conversationRepo.GetByUserAndGroup(userId, groupId);

    /// <summary>获取对话的完整消息列表</summary>
    public List<AiMessage> GetConversationMessages(int conversationId)
        => messageRepo.GetByConversationId(conversationId);

    /// <summary>重命名对话</summary>
    public bool RenameConversation(int conversationId, string title, out string? errmsg)
    {
        errmsg = null;
        var conv = conversationRepo.Existing.FirstOrDefault(x => x.Id == conversationId);
        if (conv is null) { errmsg = "对话不存在"; return false; }
        conv.Title = title;
        conversationRepo.Update(conv);
        return true;
    }

    /// <summary>删除对话（软删除）</summary>
    public bool DeleteConversation(int conversationId, out string? errmsg)
    {
        errmsg = null;
        var conv = conversationRepo.Existing.FirstOrDefault(x => x.Id == conversationId);
        if (conv is null) { errmsg = "对话不存在"; return false; }
        conversationRepo.Remove(conv);
        return true;
    }

    private List<AiMessage> LoadHistoryMessages(int conversationId, int maxContextMessages)
    {
        var all = messageRepo.GetByConversationId(conversationId);
        if (maxContextMessages > 0 && all.Count > maxContextMessages)
        {
            return all.TakeLast(maxContextMessages).ToList();
        }
        return all;
    }

    private ChatMessage ConvertToChatMessage(AiMessage msg)
    {
        var role = msg.Role switch
        {
            AiMessageRole.System => ChatRole.System,
            AiMessageRole.User => ChatRole.User,
            AiMessageRole.Assistant => ChatRole.Assistant,
            AiMessageRole.Tool => ChatRole.Tool,
            _ => ChatRole.User
        };
        return new ChatMessage(role, msg.Content ?? "");
    }

    private void SaveMessages(int conversationId, string userPrompt, string aiResponse, int existingCount)
    {
        var baseOrder = existingCount;

        // 保存用户消息
        messageRepo.Add(new AiMessage
        {
            ConversationId = conversationId,
            Role = AiMessageRole.User,
            Content = userPrompt,
            Order = baseOrder + 1
        });

        // 保存 AI 回复
        messageRepo.Add(new AiMessage
        {
            ConversationId = conversationId,
            Role = AiMessageRole.Assistant,
            Content = aiResponse,
            Order = baseOrder + 2
        });

        // 更新对话消息数和更新时间
        var conv = conversationRepo.Existing.First(x => x.Id == conversationId);
        conv.MessageCount = existingCount + 2;
        conversationRepo.Update(conv);
    }
}
```

> **注意**：`Microsoft.Extensions.AI` 的 Tool 调用在 9.x 版本中通过 `AIFunctionFactory.Create` 支持将 .NET 方法自动转换为 AI 可调用的 Function。需要确保方法参数和返回值可序列化。

### 3.4 Controller 层

#### 3.4.1 `AiChatController`

```csharp
namespace FCloud3.App.Controllers.Ai
{
    [Authorize]
    public class AiChatController(
        AiChatService aiChatService,
        HttpUserInfoService httpUserInfoService) : Controller
    {
        /// <summary>流式获取 AI 建议。conversationId 为 null 时不保存历史。</summary>
        [RateLimited(slidingWindowMs: 5000, maxCountWithin: 2)]
        public async IAsyncEnumerable<string> GetSuggestions(
            int groupId, string prompt, int? conversationId, string? currentWikiPathName,
            [EnumeratorCancellation] CancellationToken ct)
        {
            await foreach (var chunk in aiChatService.GetSuggestions(
                groupId, prompt, conversationId, currentWikiPathName, ct))
            {
                yield return chunk;
            }
        }

        /// <summary>创建新对话</summary>
        public IActionResult CreateConversation(int groupId, string? title, string? currentWikiPathName)
        {
            var userId = httpUserInfoService.GetUserId();
            var conv = aiChatService.CreateConversation(userId, groupId, title, currentWikiPathName);
            return this.ApiResp(conv);
        }

        /// <summary>获取用户的对话列表</summary>
        public IActionResult GetConversations(int groupId)
        {
            var userId = httpUserInfoService.GetUserId();
            var list = aiChatService.GetConversations(userId, groupId);
            return this.ApiResp(list);
        }

        /// <summary>获取对话的完整消息列表</summary>
        public IActionResult GetMessages(int conversationId)
        {
            var messages = aiChatService.GetConversationMessages(conversationId);
            return this.ApiResp(messages);
        }

        /// <summary>重命名对话</summary>
        public IActionResult RenameConversation(int conversationId, string title)
        {
            var res = aiChatService.RenameConversation(conversationId, title, out var errmsg);
            if (!res) return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        /// <summary>删除对话</summary>
        public IActionResult DeleteConversation(int conversationId)
        {
            var res = aiChatService.DeleteConversation(conversationId, out var errmsg);
            if (!res) return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}
```

#### 3.4.2 `GroupAiConfigController`

```csharp
namespace FCloud3.App.Controllers.Identities
{
    [Authorize]
    public class GroupAiConfigController(
        GroupAiConfigService configService) : Controller, IAuthGrantTypeProvidedController
    {
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.UserGroup;

        [AuthGranted(formKey: nameof(groupId))]
        public IActionResult Get(int groupId)
        {
            var res = configService.GetConfig(groupId, out var errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }

        [AuthGranted(formKey: nameof(groupId))]
        [UserTypeRestricted]
        public IActionResult Set([FromBody] GroupAiConfigModel model)
        {
            var res = configService.SetConfig(model.GroupId, model.ApiBaseUrl,
                model.ApiKey, model.ModelName, model.SystemPrompt, model.Enabled, out var errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class GroupAiConfigModel : IAuthGrantableRequestModel
        {
            public int GroupId { get; set; }
            public string? ApiBaseUrl { get; set; }
            public string? ApiKey { get; set; }
            public string? ModelName { get; set; }
            public string? SystemPrompt { get; set; }
            public bool Enabled { get; set; }
            public int AuthGrantOnId => GroupId;
        }
    }
}
```

### 3.5 服务注册

在 `FCloud3.Services/AddToService.cs` 中添加：

```csharp
services.AddScoped<GroupAiConfigRepo>();
services.AddScoped<AiConversationRepo>();
services.AddScoped<AiMessageRepo>();
services.AddScoped<GroupAiConfigService>();
services.AddScoped<AiToolService>();
services.AddScoped<AiChatService>();
```

---

## 四、前端架构

### 4.1 新增文件

```
FCloud3.AppFront/FCloud3Front/src/
  models/ai/
    aiChat.ts                     # AI 对话相关类型
    groupAiConfig.ts              # 团体 AI 配置类型
    aiConversation.ts             # 对话会话类型
  pages/Ai/
    AiChatPanel.vue               # AI 对话面板组件（含对话列表）
    routes/
      routesInit.ts               # 路由注册（如需要独立页面）
      routesJump.ts
```

### 4.2 API 定义

在 `src/utils/com/api.ts` 的 `Api` 类中添加：

```typescript
ai = {
    chat: {
        getSuggestions: async (groupId: number, prompt: string, conversationId?: number, currentWikiPathName?: string) => {
            const resp = await this.httpClient.request(
                "/api/AiChat/GetSuggestions",
                "get",
                { groupId, prompt, conversationId, currentWikiPathName }
            );
            // 流式响应需要特殊处理，可能需要使用 EventSource 或 fetch
            return resp;
        },
        createConversation: async (groupId: number, title?: string, currentWikiPathName?: string) => {
            const resp = await this.httpClient.request(
                "/api/AiChat/CreateConversation",
                "post",
                { groupId, title, currentWikiPathName }
            );
            if (resp.success) {
                return resp.data as AiConversation;
            }
        },
        getConversations: async (groupId: number) => {
            const resp = await this.httpClient.request(
                "/api/AiChat/GetConversations",
                "get",
                { groupId }
            );
            if (resp.success) {
                return resp.data as AiConversation[];
            }
            return [];
        },
        getMessages: async (conversationId: number) => {
            const resp = await this.httpClient.request(
                "/api/AiChat/GetMessages",
                "get",
                { conversationId }
            );
            if (resp.success) {
                return resp.data as AiMessage[];
            }
            return [];
        },
        renameConversation: async (conversationId: number, title: string) => {
            const resp = await this.httpClient.request(
                "/api/AiChat/RenameConversation",
                "post",
                { conversationId, title }
            );
            return resp.success;
        },
        deleteConversation: async (conversationId: number) => {
            const resp = await this.httpClient.request(
                "/api/AiChat/DeleteConversation",
                "post",
                { conversationId }
            );
            return resp.success;
        }
    },
    groupConfig: {
        get: async (groupId: number) => {
            const resp = await this.httpClient.request(
                "/api/GroupAiConfig/Get",
                "get",
                { groupId }
            );
            if (resp.success) {
                return resp.data as GroupAiConfig;
            }
        },
        set: async (config: GroupAiConfig) => {
            const resp = await this.httpClient.request(
                "/api/GroupAiConfig/Set",
                "postRaw",
                config,
                "保存成功",
                true
            );
            return resp.success;
        }
    }
}
```

> **流式响应注意**：`Microsoft.Extensions.AI` 的流式输出在 HTTP 层面可以用 SSE (Server-Sent Events) 实现。需要在前端使用 `EventSource` 或 `fetch` + `ReadableStream` 来消费。当前项目的 `httpClient.ts` 可能需要扩展以支持 SSE。

### 4.3 `AiChatPanel.vue` 组件

一个可嵌入词条编辑页面的侧边栏/浮动面板：

```vue
<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { injectApi, injectPop } from '@/provides';
import type { AiConversation, AiMessage } from '@/models/ai/aiConversation';

const props = defineProps<{
    groupId: number;
    currentWikiPathName?: string;
}>();

const api = injectApi();
const pop = injectPop();

// 对话列表
const conversations = ref<AiConversation[]>([]);
const currentConversationId = ref<number | null>(null);
const currentTitle = ref('新对话');

// 消息列表
const messages = ref<Array<{role:'user'|'ai', content:string}>>([]);
const prompt = ref('');
const loading = ref(false);
const showConvList = ref(true);

onMounted(() => {
    loadConversations();
});

watch(currentConversationId, async (id) => {
    if (id) {
        await loadMessages(id);
        const conv = conversations.value.find(c => c.id === id);
        currentTitle.value = conv?.title || '对话';
    } else {
        messages.value = [];
        currentTitle.value = '新对话';
    }
});

async function loadConversations() {
    const list = await api.ai.chat.getConversations(props.groupId);
    conversations.value = list || [];
}

async function loadMessages(conversationId: number) {
    const list = await api.ai.chat.getMessages(conversationId);
    messages.value = (list || []).map((m: AiMessage) => ({
        role: m.role === 1 ? 'user' : 'ai' as 'user'|'ai',
        content: m.content || ''
    }));
}

async function createConversation() {
    const conv = await api.ai.chat.createConversation(
        props.groupId,
        undefined,
        props.currentWikiPathName
    );
    if (conv) {
        conversations.value.unshift(conv);
        currentConversationId.value = conv.id;
    }
}

async function renameConversation() {
    if (!currentConversationId.value) return;
    const newTitle = prompt('请输入新标题:', currentTitle.value);
    if (newTitle && newTitle !== currentTitle.value) {
        const ok = await api.ai.chat.renameConversation(currentConversationId.value, newTitle);
        if (ok) {
            currentTitle.value = newTitle;
            await loadConversations();
        }
    }
}

async function deleteConversation(id: number) {
    const ok = await api.ai.chat.deleteConversation(id);
    if (ok) {
        conversations.value = conversations.value.filter(c => c.id !== id);
        if (currentConversationId.value === id) {
            currentConversationId.value = null;
        }
    }
}

async function send() {
    if (!prompt.value.trim()) return;

    // 如果没有当前对话，先自动创建一个
    if (!currentConversationId.value) {
        await createConversation();
        if (!currentConversationId.value) return;
    }

    const userMsg = prompt.value;
    messages.value.push({ role: 'user', content: userMsg });
    prompt.value = '';
    loading.value = true;

    try {
        const response = await fetchSuggestions(userMsg);
        messages.value.push({ role: 'ai', content: response });
        // 更新对话列表中的消息数
        await loadConversations();
    } catch (e) {
        pop.value.show('AI 请求失败', 'failed');
    } finally {
        loading.value = false;
    }
}

async function fetchSuggestions(userPrompt: string): Promise<string> {
    const url = new URL(`${import.meta.env.VITE_ApiUrlBase}/api/AiChat/GetSuggestions`);
    url.searchParams.set('groupId', props.groupId.toString());
    url.searchParams.set('prompt', userPrompt);
    if (currentConversationId.value) {
        url.searchParams.set('conversationId', currentConversationId.value.toString());
    }
    if (props.currentWikiPathName) {
        url.searchParams.set('currentWikiPathName', props.currentWikiPathName);
    }

    const eventSource = new EventSource(url.toString());
    let result = '';
    return new Promise((resolve, reject) => {
        eventSource.onmessage = (e) => {
            result += e.data;
        };
        eventSource.onerror = (e) => {
            eventSource.close();
            if (result) resolve(result);
            else reject(e);
        };
    });
}
</script>

<template>
    <div class="aiChatPanel">
        <!-- 对话列表侧边栏 -->
        <div v-if="showConvList" class="convList">
            <div class="convHeader">
                <button @click="createConversation">+ 新建对话</button>
            </div>
            <div v-for="c in conversations" :key="c.id"
                 :class="['convItem', { active: c.id === currentConversationId }]"
                 @click="currentConversationId = c.id">
                <span class="convTitle">{{ c.title || '未命名对话' }}</span>
                <span class="convCount">({{ c.messageCount }})</span>
                <button class="delBtn" @click.stop="deleteConversation(c.id)">×</button>
            </div>
        </div>

        <!-- 主聊天区域 -->
        <div class="chatMain">
            <div class="chatHeader">
                <button @click="showConvList = !showConvList">☰</button>
                <span class="title" @click="renameConversation">{{ currentTitle }}</span>
            </div>
            <div class="messages">
                <div v-for="m in messages" :class="['msg', m.role]">
                    {{ m.content }}
                </div>
            </div>
            <div class="inputArea">
                <textarea v-model="prompt" placeholder="向 AI 询问创作建议..." @keydown.enter.prevent="send"/>
                <button @click="send" :disabled="loading">发送</button>
            </div>
        </div>
    </div>
</template>

<style scoped lang="scss">
.aiChatPanel {
    display: flex;
    height: 100%;
}
.convList {
    width: 200px;
    border-right: 1px solid #ddd;
    overflow-y: auto;
    .convHeader {
        padding: 8px;
        border-bottom: 1px solid #eee;
        button { width: 100%; }
    }
    .convItem {
        padding: 8px 12px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 4px;
        &:hover, &.active { background: #f0f0f0; }
        .convTitle { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .convCount { color: #999; font-size: 12px; }
        .delBtn {
            display: none;
            background: none;
            border: none;
            color: #999;
            cursor: pointer;
        }
        &:hover .delBtn { display: inline; }
    }
}
.chatMain {
    flex: 1;
    display: flex;
    flex-direction: column;
    .chatHeader {
        padding: 8px 12px;
        border-bottom: 1px solid #eee;
        display: flex;
        align-items: center;
        gap: 8px;
        .title {
            font-weight: bold;
            cursor: pointer;
            &:hover { text-decoration: underline; }
        }
    }
}
.messages {
    flex-grow: 1;
    overflow-y: auto;
}
.msg {
    padding: 8px;
    margin: 4px;
    border-radius: 4px;
}
.msg.user {
    background: #e3f2fd;
    text-align: right;
}
.msg.ai {
    background: #f5f5f5;
    text-align: left;
}
.inputArea {
    display: flex;
    gap: 8px;
    padding: 8px;
    textarea {
        flex-grow: 1;
        resize: none;
        height: 60px;
    }
}
</style>
```

### 4.4 集成到词条编辑页面

在 `WikiItemContentEdit.vue` 或 `ViewWiki.vue` 中添加 AI 面板入口：

- 在词条编辑页面侧边栏添加"AI 助手"按钮
- 点击后展开 `AiChatPanel`，传入当前词条编辑的 `pathName` 和用户选择的团体 `groupId`

### 4.5 团体 AI 配置 UI

在 `UserGroupDetail.vue` 中添加 AI 配置编辑区域（仅团体所有者/管理员可见）：

```vue
<div v-if="data.CanEdit">
    <h3>AI 配置</h3>
    <table>
        <tr><td>API 地址</td><td><input v-model="aiConfig.apiBaseUrl"/></td></tr>
        <tr><td>API Key</td><td><input v-model="aiConfig.apiKey" type="password"/></td></tr>
        <tr><td>模型名</td><td><input v-model="aiConfig.modelName"/></td></tr>
        <tr><td>系统提示词</td><td><textarea v-model="aiConfig.systemPrompt"/></td></tr>
        <tr><td>启用</td><td><input type="checkbox" v-model="aiConfig.enabled"/></td></tr>
        <tr><td colspan="2"><button @click="saveAiConfig">保存</button></td></tr>
    </table>
</div>
```

---

## 五、用量统计与计费

### 5.1 统计维度

每次 AI 调用需要记录以下信息：

| 字段 | 说明 |
|------|------|
| 用户Id | 谁发起的调用 |
| 团体Id | 使用了哪个团体的配置 |
| 调用时间 | 精确到秒 |
| 输入Token数 | 用户Prompt + 系统提示词 + 工具返回内容的总Token |
| 输出Token数 | AI 回复内容的Token数 |
| 总Token数 | 输入 + 输出 |
| 调用结果 | 成功/失败/超时 |
| 使用的模型 | 如 gpt-4o |
| 用户Prompt摘要 | 用户原始 prompt 的前100个字符（用于审计和用量列表展示） |
| 关联词条 | 当前正在编辑的词条路径名（如有） |
| 关联对话 | 当前对话的 Id（如有） |

### 5.2 推荐方案：Token 计数（最准确）

**方案A：使用 `Microsoft.Extensions.AI` 内置的 Usage 信息（推荐）**

MEAI 的 `ChatResponse` 和 `ChatResponseUpdate` 对象通常包含 `Usage` 属性，可直接获取 `InputTokenCount` 和 `OutputTokenCount`：

```csharp
public class AiChatService(...)
{
    public async IAsyncEnumerable<AiChatChunk> GetSuggestions(
        int groupId, int userId, string userPrompt, string? currentWikiPathName,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var config = ...;
        var chatClient = ...;
        var messages = ...;
        var tools = ...;

        // 用于累积完整的输入和输出
        string fullResponse = "";
        ChatTokenUsage? finalUsage = null;

        await foreach (var update in chatClient.GetStreamingResponseAsync(
            messages, new() { Tools = tools }, ct))
        {
            fullResponse += update.Text ?? "";
            if (update.Usage is not null)
            {
                finalUsage = update.Usage;
            }
            yield return new AiChatChunk(update.Text ?? "");
        }

        // 流结束后，记录用量
        var inputTokens = finalUsage?.InputTokenCount ?? EstimateTokens(messages);
        var outputTokens = finalUsage?.OutputTokenCount ?? EstimateTokens(fullResponse);

        await aiUsageRecordService.Record(new AiUsageRecord
        {
            UserId = userId,
            GroupId = groupId,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            TotalTokens = inputTokens + outputTokens,
            ModelName = config.ModelName,
            Success = true,
            // PromptSummary 直接来源于用户传入的 prompt 参数
            // 用于后台审计和用量列表展示，让管理员知道这次调用是关于什么主题
            PromptSummary = userPrompt[..Math.Min(100, userPrompt.Length)],
            RelatedWikiPathName = currentWikiPathName,
            ConversationId = conversationId
        });
    }
}

public record AiChatChunk(string Text);
```

**优点**：
- 由 AI 提供商返回的真实 Token 数，最准确
- 输入Token包含了系统提示词、用户Prompt、工具返回内容

**缺点**：
- 部分提供商/模型可能不返回 Usage 信息（尤其是流式响应的中间 chunk）
- 需要在流结束后才能拿到最终用量，记录时机较晚

### 5.3 备选方案：本地 Token 估算

如果提供商不返回 Usage，可以使用 `Tiktoken` 或 `Microsoft.ML.Tokenizers` 进行本地估算：

```csharp
// 添加 NuGet 包：Microsoft.ML.Tokenizers
using Microsoft.ML.Tokenizers;

public class TokenEstimator
{
    private readonly Tokenizer _tokenizer;

    public TokenEstimator()
    {
        // 使用 cl100k_base（GPT-4/GPT-3.5 的编码器）
        _tokenizer = Tokenizer.CreateTiktokenForModel("gpt-4");
    }

    public int CountTokens(string text)
    {
        return _tokenizer.CountTokens(text);
    }

    public int CountTokens(IEnumerable<ChatMessage> messages)
    {
        int total = 0;
        foreach (var msg in messages)
        {
            total += _tokenizer.CountTokens(msg.Text);
            // 每条消息有固定的格式开销（约4 tokens）
            total += 4;
        }
        return total;
    }
}
```

**优点**：
- 不依赖提供商返回 Usage
- 可以在发送请求前预估用量，用于前置配额检查

**缺点**：
- 是估算值，与真实值可能有偏差
- 不同模型的编码器不同，需要匹配

### 5.4 推荐做法：混合方案

```csharp
public class AiUsageRecorder(
    AiUsageRecordRepo repo,
    TokenEstimator estimator,
    ILogger<AiUsageRecorder> logger)
{
    public async Task Record(int userId, int groupId, string modelName,
        List<ChatMessage> messages, string response, bool success,
        string? promptSummary, string? relatedWikiPathName,
        ChatTokenUsage? providerUsage = null)
    {
        int inputTokens, outputTokens;

        if (providerUsage is not null)
        {
            // 优先使用提供商返回的真实数据
            inputTokens = providerUsage.InputTokenCount;
            outputTokens = providerUsage.OutputTokenCount;
        }
        else
        {
            //  fallback 到本地估算
            inputTokens = estimator.CountTokens(messages);
            outputTokens = estimator.CountTokens(response);
            logger.LogWarning("AI 用量使用本地估算：user={userId}, group={groupId}", userId, groupId);
        }

        var record = new AiUsageRecord
        {
            UserId = userId,
            GroupId = groupId,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            TotalTokens = inputTokens + outputTokens,
            ModelName = modelName,
            Success = success,
            // promptSummary 由调用方从用户原始 prompt 截取前100字符传入
            // 仅用于审计和列表展示，不用于 Token 计算
            PromptSummary = promptSummary,
            RelatedWikiPathName = relatedWikiPathName
        };

        repo.Add(record);
    }
}
```

### 5.5 数据库实体：`AiUsageRecord`

```csharp
namespace FCloud3.Entities.Ai
{
    public class AiUsageRecord : IDbModel
    {
        public int Id { get; set; }
        /// <summary>调用用户Id</summary>
        public int UserId { get; set; }
        /// <summary>使用的团体Id</summary>
        public int GroupId { get; set; }
        /// <summary>输入Token数</summary>
        public int InputTokens { get; set; }
        /// <summary>输出Token数</summary>
        public int OutputTokens { get; set; }
        /// <summary>总Token数</summary>
        public int TotalTokens { get; set; }
        /// <summary>使用的模型名称</summary>
        public string? ModelName { get; set; }
        /// <summary>调用是否成功</summary>
        public bool Success { get; set; }
        /// <summary>用户Prompt摘要（前100字符），从用户传入的 prompt 参数截取</summary>
        public string? PromptSummary { get; set; }
        /// <summary>关联的词条路径名</summary>
        public string? RelatedWikiPathName { get; set; }
        /// <summary>关联的对话Id（可为空）</summary>
        public int? ConversationId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
```

### 5.6 用量查询与限额

**Service 层提供的方法**：

```csharp
public class AiUsageService(AiUsageRecordRepo repo)
{
    /// <summary>查询某用户在某团体的累计用量</summary>
    public int GetUserGroupUsage(int userId, int groupId, DateTime? since = null)
    {
        since ??= DateTime.Now.Date;
        return repo.Existing
            .Where(x => x.UserId == userId && x.GroupId == groupId && x.Created >= since)
            .Sum(x => x.TotalTokens);
    }

    /// <summary>查询某团体的累计用量（所有成员）</summary>
    public int GetGroupUsage(int groupId, DateTime? since = null)
    {
        since ??= DateTime.Now.Date;
        return repo.Existing
            .Where(x => x.GroupId == groupId && x.Created >= since)
            .Sum(x => x.TotalTokens);
    }

    /// <summary>查询某用户在各团体的用量排行</summary>
    public List<GroupUsageSummary> GetGroupUsageRanking(int groupId, DateTime? since = null)
    {
        since ??= DateTime.Now.Date;
        return repo.Existing
            .Where(x => x.GroupId == groupId && x.Created >= since)
            .GroupBy(x => x.UserId)
            .Select(g => new GroupUsageSummary(g.Key, g.Sum(x => x.TotalTokens), g.Count()))
            .OrderByDescending(x => x.TotalTokens)
            .ToList();
    }
}

public record GroupUsageSummary(int UserId, int TotalTokens, int CallCount);
```

**Controller 接口**：

```csharp
[Authorize]
public class AiUsageController(AiUsageService usageService) : Controller
{
    /// <summary>获取当前用户在指定团体的今日用量</summary>
    public IActionResult MyUsage(int groupId)
    {
        var total = usageService.GetUserGroupUsage(userId, groupId);
        return this.ApiResp(new { TotalTokens = total });
    }

    /// <summary>团体管理员查看团体用量排行</summary>
    [AuthGranted(formKey: nameof(groupId))]
    public IActionResult GroupRanking(int groupId)
    {
        var ranking = usageService.GetGroupUsageRanking(groupId);
        return this.ApiResp(ranking);
    }
}
```

### 5.7 用量限额控制

在 `GroupAiConfig` 中添加限额字段：

```csharp
public class GroupAiConfig : IDbModel
{
    // ... 原有字段 ...
    /// <summary>默认 AI 可查看的目录范围（某目录及其子级），0 表示不限</summary>
    public int DefaultDirId { get; set; }
    /// <summary>最大上下文消息数（0表示不限制）</summary>
    public int MaxContextMessages { get; set; }
    /// <summary>每日Token限额（0表示不限）</summary>
    public int DailyTokenLimit { get; set; }
    /// <summary>每月Token限额（0表示不限）</summary>
    public int MonthlyTokenLimit { get; set; }
}
```

在 `AiChatService` 中调用前检查：

```csharp
public async IAsyncEnumerable<AiChatChunk> GetSuggestions(...)
{
    var config = configService.GetConfig(groupId, out _);
    if (config is null || !config.Enabled) yield break;

    // 检查团体日限额
    if (config.DailyTokenLimit > 0)
    {
        var todayUsage = usageService.GetGroupUsage(groupId, DateTime.Now.Date);
        if (todayUsage >= config.DailyTokenLimit)
        {
            yield return new AiChatChunk("本团体今日 AI 用量已达上限，请联系管理员");
            yield break;
        }
    }

    // ... 执行 AI 调用 ...
}
```

### 5.8 前端用量展示

在 `AiChatPanel.vue` 中显示当前用量：

```vue
<div class="usageInfo">
    今日用量: {{ todayUsage }} / {{ dailyLimit > 0 ? dailyLimit : '不限' }} tokens
</div>
```

---

## 六、安全与权限

### 6.1 API Key 安全

- **必须加密存储**：数据库中的 `ApiKey` 不应明文保存。可使用 ASP.NET Core 的 `IDataProtector` 进行加密/解密。
- **绝不返回给前端**：`GetConfig` 接口返回配置时，应将 `ApiKey` 脱敏（如显示为 `sk-****xxxx`）或完全不返回。

### 6.2 权限控制

| 操作 | 权限要求 |
|------|----------|
| 查看团体 AI 配置 | 团体成员 |
| 修改团体 AI 配置 | 团体所有者/管理员（`AuthGranted` + `UserTypeRestricted`） |
| 使用 AI 对话 | 团体成员（且配置已启用） |
| 查看个人用量 | 本人 |
| 查看团体用量排行 | 团体所有者/管理员 |
| 查看对话列表 | 本人 |
| 查看对话内容 | 本人（对话创建者） |
| 创建新对话 | 团体成员 |
| 继续已有对话 | 本人且对话属于该团体 |
| 删除对话 | 本人 |
| 重命名对话 | 本人 |

### 6.3 速率限制

- AI 对话接口使用 `[RateLimited]` 属性限制调用频率（建议：5 秒内最多 2 次请求）。
- 可在 `GroupAiConfig` 中配置每日/每月 Token 限额。

### 6.4 词条内容访问控制

- AI 工具获取词条内容时，必须复用现有的权限检查逻辑（`AuthGrantService.CheckAccess`）。
- AI 不应能访问用户无权限查看的隐藏/封禁词条。

---

## 六、实现步骤清单

### 阶段一：后端基础（无迁移冲突风险）

1. [ ] 添加 `Microsoft.Extensions.AI` 和 `Microsoft.Extensions.AI.OpenAI` NuGet 包
2. [ ] 创建 `GroupAiConfig` 实体类
3. [ ] 创建 `AiConversation` 实体类
4. [ ] 创建 `AiMessage` 实体类
5. [ ] 创建 `AiUsageRecord` 实体类
6. [ ] 更新 `FCloudContext`（或在独立 DbContext 中创建）
7. [ ] 创建 `GroupAiConfigRepo`
8. [ ] 创建 `AiConversationRepo`
9. [ ] 创建 `AiMessageRepo`
10. [ ] 创建 `AiUsageRecordRepo`
11. [ ] 创建 `GroupAiConfigService`
12. [ ] 创建 `AiToolService`（封装词条内容获取和搜索）
13. [ ] 创建 `AiUsageRecorder`（用量记录服务）
14. [ ] 创建 `AiUsageService`（用量查询服务）
15. [ ] 创建 `AiChatService`（集成 MEAI，实现流式对话 + 对话历史 + 用量记录）
16. [ ] 创建 `GroupAiConfigController`
17. [ ] 创建 `AiChatController`（含对话管理接口）
18. [ ] 创建 `AiUsageController`
19. [ ] 在 `AddToService.cs` 注册新服务
20. [ ] 执行 `dotnet build` 验证编译

### 阶段二：前端基础

21. [ ] 创建 `GroupAiConfig`、`AiUsageRecord`、`AiConversation`、`AiMessage` 等 TypeScript 类型定义
22. [ ] 在 `api.ts` 中添加 AI 相关 API 方法（含对话管理）
23. [ ] 创建 `AiChatPanel.vue` 组件（含对话列表、新建/切换/重命名/删除对话）
24. [ ] 在 `UserGroupDetail.vue` 中添加 AI 配置编辑 UI（含限额和上下文长度设置）
25. [ ] 在词条编辑页面集成 AI 对话入口
26. [ ] 运行 `npm run type-check` 验证类型检查

### 阶段三：测试与优化

27. [ ] 测试团体 AI 配置的 CRUD
28. [ ] 测试 AI 对话流式响应
29. [ ] 测试 AI 工具调用（获取词条内容、搜索词条）
30. [ ] 测试对话历史持久化（创建、加载、继续对话）
31. [ ] 测试对话管理（新建、切换、重命名、删除）
32. [ ] 测试上下文长度控制（MaxContextMessages 截断）
33. [ ] 测试用量记录准确性
34. [ ] 测试用量查询和限额控制
35. [ ] 测试权限控制（非成员无法使用/修改，无法查看他人对话）
36. [ ] 测试速率限制
37. [ ] 构建前端：`node FCloud3.AppFront/buildFront.mjs`

---

## 七、注意要点

### 7.1 关于 DbContext 和迁移（**极其重要**）

> 根据 `AGENTS.md` 的注意事项：
> - 如果是二次开发且需维持 upstream：**必须在单独的 DbContext 中创建新实体**，使用**分离的迁移目录**。
> - 不要修改原有的 `FCloudContext` 和已有的迁移目录。
> - 如果不确定是否维持 upstream，在修改前询问用户确认。

**建议做法**：创建 `FCloud3.DbContexts/Extended/FCloudExtendedContext.cs`，专门存放二开的新实体和迁移。

### 7.2 关于 `Microsoft.Extensions.AI` 版本

- MEAI 9.x 支持 .NET 10，与项目目标框架一致。
- Tool/Function Calling 的 API 在不同版本间可能有变化，建议查阅对应版本的官方文档。
- `AIFunctionFactory.Create` 需要方法参数可 JSON 序列化。

### 7.3 流式响应的实现

- ASP.NET Core 的 `IAsyncEnumerable<T>` 默认不会自动流式传输到客户端。
- 需要使用 `NDJson` 格式或 SSE（Server-Sent Events）来实现真正的流式输出。
- 前端消费流式响应时，可能需要绕过现有的 `httpClient.ts`，直接使用 `fetch` 或 `EventSource`。

### 7.4 API Key 加密

```csharp
// 加密示例（在 Service 中）
public class GroupAiConfigService
{
    private readonly IDataProtector _protector;
    
    public GroupAiConfigService(..., IDataProtectionProvider dataProtection)
    {
        _protector = dataProtection.CreateProtector("GroupAiConfig");
    }
    
    public string EncryptKey(string key) => _protector.Protect(key);
    public string DecryptKey(string encrypted) => _protector.Unprotect(encrypted);
}
```

需要在 `Program.cs` 中注册 `services.AddDataProtection()`。

### 7.5 词条内容提取

- `WikiParsingService.GetParsedWiki()` 返回的是解析后的 HTML 内容。
- 给 AI 使用时，建议提取纯文本（去除 HTML 标签），可在 `AiToolService` 中用正则或 HtmlAgilityPack 处理。
- 如果词条内容过长，需要考虑截断或分块，避免超出模型上下文长度。

### 7.6 团体选择

- 用户可能属于多个团体，使用 AI 时需要让用户选择使用哪个团体的配置。
- 可在 AI 面板中添加团体选择下拉框，默认选择用户最近活跃或第一个有配置且启用的团体。

### 7.7 错误处理

- AI 服务不可用、API Key 无效、网络超时等情况需要友好提示。
- 建议在前端显示"AI 服务暂时不可用"等兜底文案。

### 7.8 用量记录的异步处理

用量记录涉及数据库写入，如果在流式响应的主线程中同步写入，可能影响响应延迟。建议：

```csharp
// 使用 BackgroundService 或 Channel 异步写入
public async Task RecordAsync(AiUsageRecord record)
{
    // 方式1：直接写入（简单，适合低并发）
    repo.Add(record);

    // 方式2：后台队列（高并发场景）
    // await _channel.Writer.WriteAsync(record);
}
```

对于一般 wiki 系统的 AI 使用量，**直接同步写入即可**，因为：
- 每次 AI 调用本身就需要数百毫秒到数秒
- 数据库写入通常是亚毫秒级操作
- 过早优化不必要

### 7.9 Token 计数器选择

| 库 | 用途 | NuGet 包 |
|----|------|----------|
| `Microsoft.ML.Tokenizers` | 通用 Tokenizer，支持 Tiktoken | `Microsoft.ML.Tokenizers` |
| `Tiktoken` | OpenAI 专用，更精确 | `Tiktoken` |

对于 MEAI + OpenAI 兼容场景，推荐 `Microsoft.ML.Tokenizers`，因为它是微软官方库，与 MEAI 生态一致。

### 7.10 用量数据清理

`AiUsageRecord` 数据量可能增长很快，建议：
- 保留最近 90 天的详细记录
-  older 数据可汇总到月度统计表中后删除
- 或定期归档到单独的数据库/表中

### 7.11 对话上下文长度控制

为避免历史消息过长导致 Token 消耗爆炸和超出模型上下文窗口：

**在 `GroupAiConfig` 中配置：**
- `MaxContextMessages`：最大上下文消息数（默认 20 条，0 表示不限制）

**`AiChatService` 中的截断逻辑：**

```csharp
private List<AiMessage> LoadHistoryMessages(int conversationId, int maxContextMessages)
{
    var all = messageRepo.GetByConversationId(conversationId);
    if (maxContextMessages > 0 && all.Count > maxContextMessages)
    {
        // 只保留最近 N 条消息
        return all.TakeLast(maxContextMessages).ToList();
    }
    return all;
}
```

**进阶方案（按 Token 截断）：**

```csharp
private List<AiMessage> LoadHistoryMessagesByToken(int conversationId, int maxContextTokens)
{
    var all = messageRepo.GetByConversationId(conversationId);
    if (maxContextTokens <= 0) return all;

    var result = new List<AiMessage>();
    int totalTokens = 0;
    // 从后往前累加，直到达到 Token 上限
    for (int i = all.Count - 1; i >= 0; i--)
    {
        var msg = all[i];
        totalTokens += msg.TokenCount;
        if (totalTokens > maxContextTokens && result.Count > 0)
            break;
        result.Insert(0, msg);
    }
    return result;
}
```

**建议：**
- 初期使用按消息数截断（简单可控）
- 后期可按 Token 数截断（更精确，避免长消息占满上下文）
- 系统提示词和词条内容不计入 `MaxContextMessages` 限制

---

## 八、参考代码位置

| 功能 | 文件路径 |
|------|----------|
| 词条内容获取（Controller） | `FCloud3.App/Controllers/WikiParsing/WikiParsingController.cs` |
| 词条内容获取（Service） | `FCloud3.Services/WikiParsing/WikiParsingService.cs` |
| 词条搜索 | `FCloud3.App/Controllers/Etc/QuickSearchController.cs` |
| 团体管理（Controller） | `FCloud3.App/Controllers/Identities/UserGroupController.cs` |
| 团体管理（Service） | `FCloud3.Services/Identities/UserGroupService.cs` |
| 权限过滤 | `FCloud3.App/Services/Filters/AuthGrantActionFilter.cs` |
| 速率限制 | `FCloud3.App/Services/Filters/RateLimiterFilter.cs` |
| 操作记录（参考） | `FCloud3.Entities/Messages/OpRecord.cs` |
| 前端 API 定义 | `FCloud3.AppFront/FCloud3Front/src/utils/com/api.ts` |
| 前端团体详情 | `FCloud3.AppFront/FCloud3Front/src/pages/Identities/UserGroupDetail.vue` |
| 前端词条编辑 | `FCloud3.AppFront/FCloud3Front/src/pages/Wiki/WikiItemContentEdit.vue` |
