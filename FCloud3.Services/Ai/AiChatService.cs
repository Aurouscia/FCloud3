using System.Runtime.CompilerServices;
using FCloud3.Entities.Ai;
using FCloud3.Repos.Ai;
using FCloud3.Services.Identities;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace FCloud3.Services.Ai
{
    public class AiChatService(
        AiInstanceConfigService configService,
        AiToolService toolService,
        AiConversationRepo conversationRepo,
        AiMessageRepo messageRepo,
        AiUsageRecorder usageRecorder,
        AiUsageService usageService,
        IOperatingUserIdProvider userIdProvider)
    {
        private readonly int _userId = userIdProvider.Get();

        /// <summary>
        /// 获取 AI 建议。conversationId 为 null 时不保存历史（无状态模式）。
        /// </summary>
        public async IAsyncEnumerable<AiChatChunk> GetSuggestions(
            int groupId, string userPrompt, int? conversationId,
            int currentWikiItemId, [EnumeratorCancellation] CancellationToken ct)
        {
            var config = configService.GetConfig(groupId, out var errmsg);
            if (config is null)
            {
                yield return new AiChatChunk(errmsg ?? "无法获取 AI 配置");
                yield break;
            }
            if (config is null || !config.Enabled || string.IsNullOrEmpty(config.ApiKey))
            {
                yield return new AiChatChunk("AI 功能未启用或配置不完整");
                yield break;
            }

            // 检查AI实例日限额
            if (config.DailyTokenLimit > 0)
            {
                var todayUsage = usageService.GetConfigUsage(config.Id, DateTime.Now.Date);
                if (todayUsage >= config.DailyTokenLimit)
                {
                    yield return new AiChatChunk("本AI实例今日用量已达上限，请联系管理员");
                    yield break;
                }
            }

            // 构建 OpenAI 客户端（支持任意 OpenAI 兼容端点）
            var openaiClient = new OpenAIClient(
                new ApiKeyCredential(config.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(config.ApiBaseUrl!) });

            IChatClient chatClient = openaiClient
                .GetChatClient(config.ModelName!)
                .AsIChatClient();

            // 构建消息列表
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, config.SystemPrompt ?? "你是一个 wiki 创作助手，可以帮助用户改进词条内容、提供创作建议。你可以使用工具获取词条内容或搜索相关词条。")
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
            string? currentWikiPathName = null;
            if (currentWikiItemId > 0)
            {
                // TODO: 需要通过 WikiItemRepo 根据 Id 获取 pathName
                // 临时保留 pathName 用于工具调用，不在数据库中存储
                currentWikiPathName = GetWikiPathNameById(currentWikiItemId);
                if (currentWikiPathName is not null)
                {
                    var content = toolService.GetWikiContent(currentWikiPathName);
                    if (content is not null)
                    {
                        messages.Add(new ChatMessage(ChatRole.User,
                            $"当前正在编辑的词条内容：\n```\n{content}\n```"));
                    }
                }
            }

            messages.Add(new ChatMessage(ChatRole.User, userPrompt));

            // 定义工具
            var options = new ChatOptions
            {
                Tools =
                [
                    AIFunctionFactory.Create(toolService.GetWikiContent, name: "get_wiki_content"),
                    AIFunctionFactory.Create(toolService.SearchWiki, name: "search_wiki")
                ]
            };

            // 启用工具调用中间件
            chatClient = chatClient.AsBuilder().UseFunctionInvocation().Build();

            // 流式调用并收集完整回复和工具调用信息
            string fullResponse = "";
            List<AiToolCallInfo> toolCalls = [];

            await foreach (var update in chatClient.GetStreamingResponseAsync(
                messages, options, ct))
            {
                fullResponse += update.Text ?? "";
                if (update.Contents is not null)
                {
                    foreach (var content in update.Contents)
                    {
                        if (content is FunctionCallContent fc && !string.IsNullOrEmpty(fc.Name))
                        {
                            var args = fc.Arguments is not null
                                ? System.Text.Json.JsonSerializer.Serialize(fc.Arguments)
                                : "";
                            toolCalls.Add(new AiToolCallInfo(fc.Name, args));
                        }
                    }
                }
                yield return new AiChatChunk(update.Text ?? "", toolCalls.Count > 0 ? toolCalls : null);
            }

            // 保存消息到数据库（如果有 conversationId）
            if (conversationId.HasValue)
            {
                var toolCallsJson = toolCalls.Count > 0
                    ? System.Text.Json.JsonSerializer.Serialize(toolCalls)
                    : null;
                SaveMessages(conversationId.Value, userPrompt, fullResponse, history.Count, toolCallsJson);
            }

            // 记录用量（流式无 Usage，使用本地估算）
            var promptSummary = userPrompt[..Math.Min(100, userPrompt.Length)];
            usageRecorder.RecordWithFallback(_userId, config.Id, config.ModelName!,
                messages, fullResponse, true, promptSummary, currentWikiItemId,
                conversationId, null);
        }

        /// <summary>创建新对话</summary>
        public AiConversation CreateConversation(int userId, int aiInstanceConfigId,
            string? title, int currentWikiItemId)
        {
            var conversation = new AiConversation
            {
                UserId = userId,
                AiInstanceConfigId = aiInstanceConfigId,
                Title = title,
                CurrentWikiItemId = currentWikiItemId,
                MessageCount = 0
            };
            conversationRepo.AddConversation(conversation);
            return conversation;
        }

        /// <summary>获取用户的对话列表</summary>
        public List<AiConversation> GetConversations(int userId, int aiInstanceConfigId)
            => conversationRepo.GetByUserAndConfig(userId, aiInstanceConfigId);

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
            conversationRepo.UpdateConversation(conv);
            return true;
        }

        /// <summary>删除对话（软删除）</summary>
        public bool DeleteConversation(int conversationId, out string? errmsg)
        {
            errmsg = null;
            var conv = conversationRepo.Existing.FirstOrDefault(x => x.Id == conversationId);
            if (conv is null) { errmsg = "对话不存在"; return false; }
            conversationRepo.RemoveConversation(conv);
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

        private static ChatMessage ConvertToChatMessage(AiMessage msg)
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

        private void SaveMessages(int conversationId, string userPrompt, string aiResponse, int existingCount, string? toolCallsJson = null)
        {
            var baseOrder = existingCount;

            // 保存用户消息
            messageRepo.AddMessage(new AiMessage
            {
                ConversationId = conversationId,
                Role = AiMessageRole.User,
                Content = userPrompt,
                Order = baseOrder + 1
            });

            // 保存 AI 回复
            messageRepo.AddMessage(new AiMessage
            {
                ConversationId = conversationId,
                Role = AiMessageRole.Assistant,
                Content = aiResponse,
                ToolCalls = toolCallsJson,
                Order = baseOrder + 2
            });

            // 更新对话消息数和更新时间
            var conv = conversationRepo.Existing.First(x => x.Id == conversationId);
            conv.MessageCount = existingCount + 2;
            conversationRepo.UpdateConversation(conv);
        }

        private static string? GetWikiPathNameById(int wikiItemId)
        {
            // TODO: 注入 WikiItemRepo 实现根据 Id 查询 pathName
            // 临时返回 null，后续需要完善
            return null;
        }
    }

    public record AiChatChunk(string Text, List<AiToolCallInfo>? ToolCalls = null);
    public record AiToolCallInfo(string Name, string Arguments);
}
