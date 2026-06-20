using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
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
            int aiInstanceConfigId, string userPrompt, int? conversationId,
            int currentWikiItemId, [EnumeratorCancellation] CancellationToken ct)
        {
            var config = configService.GetConfig(aiInstanceConfigId, out var errmsg);
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

            // 构建消息列表
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, config.SystemPrompt ?? "你是一个 wiki 创作助手，可以帮助用户改进词条内容、提供创作建议。你可以使用工具获取词条内容或搜索相关词条。")
            };

            string? effectiveModelName = config.DefaultModelName;

            // 加载对话历史（如果有）
            List<AiMessage> history = [];
            if (conversationId.HasValue)
            {
                var conv = conversationRepo.GetById(conversationId.Value);
                if (conv is null || conv.UserId != _userId || conv.AiInstanceConfigId != aiInstanceConfigId)
                {
                    yield return new AiChatChunk("对话不存在或无权限访问");
                    yield break;
                }
                effectiveModelName = conv.ModelName ?? config.DefaultModelName;
                history = LoadHistoryMessages(conversationId.Value, config.MaxContextMessages);
                foreach (var h in history)
                {
                    messages.Add(ConvertToChatMessage(h));
                }
            }

            if (string.IsNullOrEmpty(effectiveModelName))
            {
                yield return new AiChatChunk("模型名称未设置");
                yield break;
            }

            // 构建 OpenAI 客户端（支持任意 OpenAI 兼容端点）
            var openaiClient = new OpenAIClient(
                new ApiKeyCredential(config.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(config.ApiBaseUrl!) });

            IChatClient chatClient = openaiClient
                .GetChatClient(effectiveModelName)
                .AsIChatClient();

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
                ModelId = effectiveModelName,
                Tools =
                [
                    AIFunctionFactory.Create(toolService.GetWikiContent, name: "get_wiki_content"),
                    AIFunctionFactory.Create(toolService.SearchWiki, name: "search_wiki")
                ]
            };

            // 启用工具调用中间件
            chatClient = chatClient.AsBuilder().UseFunctionInvocation().Build();

            var streamingContext = new StreamingContext();
            var channel = Channel.CreateUnbounded<AiChatChunk>();
            var stopwatch = Stopwatch.StartNew();

            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, ct))
                    {
                        streamingContext.FullResponse += update.Text ?? "";
                        if (update.FinishReason is not null)
                            streamingContext.FinishReason = update.FinishReason.ToString();
                        if (update.Contents is not null)
                        {
                            foreach (var content in update.Contents)
                            {
                                if (content is FunctionCallContent fc && !string.IsNullOrEmpty(fc.Name))
                                {
                                    var args = fc.Arguments is not null
                                        ? System.Text.Json.JsonSerializer.Serialize(fc.Arguments)
                                        : "";
                                    streamingContext.ToolCalls.Add(new AiToolCallInfo(fc.Name, args));
                                }
                            }
                        }
                        await channel.Writer.WriteAsync(new AiChatChunk(
                            update.Text ?? "",
                            streamingContext.ToolCalls.Count > 0 ? streamingContext.ToolCalls : null), ct);
                    }
                    streamingContext.Success = true;
                }
                catch (OperationCanceledException)
                {
                    // 用户取消不记为失败
                }
                catch (Exception ex)
                {
                    streamingContext.Success = false;
                    streamingContext.ErrorMessage = ex.Message;
                    await channel.Writer.WriteAsync(new AiChatChunk($"请求失败：{ex.Message}"), ct);
                }
                finally
                {
                    stopwatch.Stop();
                    streamingContext.DurationMs = (int)stopwatch.ElapsedMilliseconds;
                    streamingContext.InputTokenCount = AiUsageRecorder.EstimateTokens(messages);
                    streamingContext.OutputTokenCount = EstimateOutputTokens(streamingContext.FullResponse);
                    channel.Writer.Complete();
                }
            }, ct);

            await foreach (var chunk in channel.Reader.ReadAllAsync(ct))
            {
                yield return chunk;
            }

            if (!streamingContext.Success)
            {
                if (conversationId.HasValue)
                {
                    var toolCallsJson = streamingContext.ToolCalls.Count > 0
                        ? System.Text.Json.JsonSerializer.Serialize(streamingContext.ToolCalls)
                        : null;
                    SaveMessages(conversationId.Value, userPrompt, streamingContext.FullResponse, history.Count,
                        effectiveModelName, AiMessageStatus.Failed, streamingContext.ErrorMessage,
                        streamingContext.DurationMs, streamingContext.FinishReason,
                        streamingContext.InputTokenCount, streamingContext.OutputTokenCount, toolCallsJson);
                }
                usageRecorder.RecordWithFallback(_userId, config.Id, effectiveModelName,
                    messages, streamingContext.FullResponse, false, userPrompt[..Math.Min(100, userPrompt.Length)],
                    currentWikiItemId, conversationId, null, streamingContext.DurationMs);
                yield break;
            }

            // 保存消息到数据库（如果有 conversationId）
            if (conversationId.HasValue)
            {
                var toolCallsJson = streamingContext.ToolCalls.Count > 0
                    ? System.Text.Json.JsonSerializer.Serialize(streamingContext.ToolCalls)
                    : null;
                if (!SaveMessages(conversationId.Value, userPrompt, streamingContext.FullResponse, history.Count,
                    effectiveModelName, AiMessageStatus.Received, null,
                    streamingContext.DurationMs, streamingContext.FinishReason,
                    streamingContext.InputTokenCount, streamingContext.OutputTokenCount, toolCallsJson))
                {
                    yield return new AiChatChunk("保存消息失败，可能是内容过长");
                    yield break;
                }
            }

            // 记录用量（流式无 Usage，使用本地估算）
            var promptSummary = userPrompt[..Math.Min(100, userPrompt.Length)];
            usageRecorder.RecordWithFallback(_userId, config.Id, effectiveModelName,
                messages, streamingContext.FullResponse, true, promptSummary, currentWikiItemId,
                conversationId, null, streamingContext.DurationMs);
        }

        /// <summary>创建新对话</summary>
        public bool CreateConversation(int aiInstanceConfigId, string? title, string? modelName,
            int currentWikiItemId, out AiConversation? conversation, out string? errmsg)
        {
            conversation = null;
            var config = configService.GetConfig(aiInstanceConfigId, out errmsg);
            if (config is null)
                return false;

            conversation = new AiConversation
            {
                UserId = _userId,
                AiInstanceConfigId = aiInstanceConfigId,
                Title = title,
                ModelName = string.IsNullOrWhiteSpace(modelName) ? null : modelName,
                CurrentWikiItemId = currentWikiItemId,
                MessageCount = 0
            };
            return conversationRepo.TryAddConversation(conversation, out errmsg);
        }

        /// <summary>获取用户的对话列表</summary>
        public List<AiConversation>? GetConversations(int aiInstanceConfigId, bool includeArchived, out string? errmsg)
        {
            var config = configService.GetConfig(aiInstanceConfigId, out errmsg);
            if (config is null)
                return null;
            errmsg = null;
            if (includeArchived)
                return conversationRepo.GetByUserAndConfigWithArchived(_userId, aiInstanceConfigId);
            return conversationRepo.GetByUserAndConfig(_userId, aiInstanceConfigId);
        }

        /// <summary>获取对话的完整消息列表</summary>
        public List<AiMessage>? GetConversationMessages(int conversationId, out string? errmsg)
        {
            errmsg = null;
            var conv = conversationRepo.GetById(conversationId);
            if (conv is null)
            {
                errmsg = "对话不存在";
                return null;
            }
            if (conv.UserId != _userId)
            {
                errmsg = "无权查看该对话";
                return null;
            }
            return messageRepo.GetByConversationId(conversationId);
        }

        /// <summary>重命名对话</summary>
        public bool RenameConversation(int conversationId, string title, out string? errmsg)
        {
            errmsg = null;
            var conv = conversationRepo.GetById(conversationId);
            if (conv is null)
            {
                errmsg = "对话不存在";
                return false;
            }
            if (conv.UserId != _userId)
            {
                errmsg = "无权修改该对话";
                return false;
            }
            conv.Title = title;
            return conversationRepo.TryUpdateConversation(conv, out errmsg);
        }

        /// <summary>设置对话顶置状态</summary>
        public bool SetConversationPinned(int conversationId, bool isPinned, out string? errmsg)
        {
            errmsg = null;
            var conv = conversationRepo.GetById(conversationId);
            if (conv is null)
            {
                errmsg = "对话不存在";
                return false;
            }
            if (conv.UserId != _userId)
            {
                errmsg = "无权修改该对话";
                return false;
            }
            conv.IsPinned = isPinned;
            return conversationRepo.TryUpdateConversation(conv, out errmsg);
        }

        /// <summary>设置对话归档状态</summary>
        public bool SetConversationArchived(int conversationId, bool isArchived, out string? errmsg)
        {
            errmsg = null;
            var conv = conversationRepo.GetById(conversationId);
            if (conv is null)
            {
                errmsg = "对话不存在";
                return false;
            }
            if (conv.UserId != _userId)
            {
                errmsg = "无权修改该对话";
                return false;
            }
            conv.IsArchived = isArchived;
            return conversationRepo.TryUpdateConversation(conv, out errmsg);
        }

        /// <summary>获取指定 OpenAI 兼容端点下的可用模型列表</summary>
        public async Task<(List<string>? Models, string? ErrorMessage)> GetAvailableModels(string apiBaseUrl, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl) || string.IsNullOrWhiteSpace(apiKey))
                return (null, "API 地址和 Key 不能为空");

            try
            {
                var client = new OpenAIClient(
                    new ApiKeyCredential(apiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(apiBaseUrl) });

                var modelClient = client.GetOpenAIModelClient();
                var result = await modelClient.GetModelsAsync();

                return (result.Value
                    .Select(m => m.Id)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .OrderBy(id => id)
                    .ToList(), null);
            }
            catch (System.ClientModel.ClientResultException ex) when (ex.Status == 401)
            {
                return (null, "API Key 无效或已失效");
            }
            catch (System.ClientModel.ClientResultException ex) when (ex.Status == 404)
            {
                return (null, "API 地址不存在或该端点不支持获取模型列表");
            }
            catch (Exception ex)
            {
                return (null, $"获取模型列表失败：{ex.Message}");
            }
        }

        /// <summary>删除对话（软删除）</summary>
        public bool DeleteConversation(int conversationId, out string? errmsg)
        {
            errmsg = null;
            var conv = conversationRepo.GetById(conversationId);
            if (conv is null)
            {
                errmsg = "对话不存在";
                return false;
            }
            if (conv.UserId != _userId)
            {
                errmsg = "无权删除该对话";
                return false;
            }
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

        private bool SaveMessages(int conversationId, string userPrompt, string aiResponse, int existingCount,
            string modelName, AiMessageStatus aiStatus, string? errorMessage, int durationMs, string? finishReason,
            int inputTokenCount, int outputTokenCount, string? toolCallsJson = null)
        {
            var baseOrder = existingCount;

            // 保存用户消息
            if (!messageRepo.TryAddMessage(new AiMessage
            {
                ConversationId = conversationId,
                Role = AiMessageRole.User,
                Content = userPrompt,
                ModelName = modelName,
                Status = AiMessageStatus.Sent,
                InputTokenCount = inputTokenCount,
                OutputTokenCount = 0,
                Order = baseOrder + 1
            }, out _))
                return false;

            // 保存 AI 回复
            if (!messageRepo.TryAddMessage(new AiMessage
            {
                ConversationId = conversationId,
                Role = AiMessageRole.Assistant,
                Content = aiResponse,
                ToolCalls = toolCallsJson,
                ModelName = modelName,
                Status = aiStatus,
                ErrorMessage = errorMessage,
                DurationMs = durationMs,
                FinishReason = finishReason,
                InputTokenCount = 0,
                OutputTokenCount = outputTokenCount,
                Order = baseOrder + 2
            }, out _))
                return false;

            // 更新对话消息数和更新时间
            var conv = conversationRepo.Existing.First(x => x.Id == conversationId);
            conv.MessageCount = existingCount + 2;
            return conversationRepo.TryUpdateConversation(conv, out _);
        }

        private static int EstimateOutputTokens(string text)
        {
            // 粗略估算：1 token ≈ 0.5 个字符
            int charCount = text.Length;
            return Math.Max(0, (int)(charCount * 0.5));
        }

        private static string? GetWikiPathNameById(int wikiItemId)
        {
            // TODO: 注入 WikiItemRepo 实现根据 Id 查询 pathName
            // 临时返回 null，后续需要完善
            return null;
        }

        private class StreamingContext
        {
            public string FullResponse { get; set; } = "";
            public List<AiToolCallInfo> ToolCalls { get; set; } = [];
            public string? FinishReason { get; set; }
            public bool Success { get; set; } = true;
            public string? ErrorMessage { get; set; }
            public int DurationMs { get; set; }
            public int InputTokenCount { get; set; }
            public int OutputTokenCount { get; set; }
        }
    }

    public record AiChatChunk(string Text, List<AiToolCallInfo>? ToolCalls = null);
    public record AiToolCallInfo(string Name, string Arguments);
}
