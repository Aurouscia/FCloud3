using FCloud3.Entities.Ai;
using FCloud3.Repos.Ai;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FCloud3.Services.Ai
{
    public class AiUsageRecorder(
        AiUsageRecordRepo repo,
        ILogger<AiUsageRecorder> logger)
    {
        public void Record(int userId, int aiInstanceConfigId, string modelName,
            int inputTokens, int outputTokens, bool success,
            string? promptSummary, int relatedWikiItemId, int? conversationId = null)
        {
            var record = new AiUsageRecord
            {
                UserId = userId,
                AiInstanceConfigId = aiInstanceConfigId,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = inputTokens + outputTokens,
                ModelName = modelName,
                Success = success,
                PromptSummary = promptSummary,
                RelatedWikiItemId = relatedWikiItemId,
                ConversationId = conversationId
            };

            repo.AddRecord(record);
        }

        public void RecordWithFallback(int userId, int aiInstanceConfigId, string modelName,
            List<ChatMessage> messages, string response, bool success,
            string? promptSummary, int relatedWikiItemId, int? conversationId = null,
            UsageDetails? providerUsage = null)
        {
            int inputTokens, outputTokens;

            if (providerUsage is not null)
            {
                inputTokens = (int)(providerUsage.InputTokenCount ?? 0);
                outputTokens = (int)(providerUsage.OutputTokenCount ?? 0);
            }
            else
            {
                inputTokens = EstimateTokens(messages);
                outputTokens = EstimateTokens(response);
                logger.LogWarning("AI 用量使用本地估算：user={userId}, config={configId}", userId, aiInstanceConfigId);
            }

            Record(userId, aiInstanceConfigId, modelName, inputTokens, outputTokens,
                success, promptSummary, relatedWikiItemId, conversationId);
        }

        private static int EstimateTokens(string text)
        {
            // 粗略估算：1 token ≈ 0.75 个英文字符 或 0.375 个中文字符
            // 这里使用简单字符数估算作为 fallback
            int charCount = text.Length;
            return Math.Max(1, (int)(charCount * 0.5));
        }

        private static int EstimateTokens(List<ChatMessage> messages)
        {
            int total = 0;
            foreach (var msg in messages)
            {
                total += EstimateTokens(msg.Text ?? "");
                total += 4; // 每条消息格式开销
            }
            return total;
        }

        public static int EstimateTokens(IList<ChatMessage> messages)
        {
            int total = 0;
            foreach (var msg in messages)
            {
                total += EstimateTokens(msg.Text ?? "");
                total += 4;
            }
            return total;
        }
    }
}
