using System.Runtime.CompilerServices;
using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Ai
{
    [Authorize]
    public class AiChatController(
        AiChatService aiChatService) : Controller
    {
        /// <summary>流式获取 AI 建议。conversationId 为 null 时不保存历史。</summary>
        [RateLimited(slidingWindowMs: 5000, maxCountWithin: 2)]
        public async IAsyncEnumerable<AiChatChunk> GetSuggestions(
            int aiInstanceConfigId, string prompt, int? conversationId, int currentWikiItemId,
            [EnumeratorCancellation] CancellationToken ct)
        {
            await foreach (var chunk in aiChatService.GetSuggestions(
                aiInstanceConfigId, prompt, conversationId, currentWikiItemId, ct))
            {
                yield return chunk;
            }
        }

        /// <summary>创建新对话</summary>
        public IActionResult CreateConversation(int aiInstanceConfigId, string? title, string? modelName, int currentWikiItemId)
        {
            var res = aiChatService.CreateConversation(aiInstanceConfigId, title, modelName, currentWikiItemId,
                out var conv, out var errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(conv);
        }

        /// <summary>获取用户的对话列表</summary>
        public IActionResult GetConversations(int aiInstanceConfigId)
        {
            var list = aiChatService.GetConversations(aiInstanceConfigId, out var errmsg);
            if (list is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(list);
        }

        /// <summary>获取对话的完整消息列表</summary>
        public IActionResult GetMessages(int conversationId)
        {
            var messages = aiChatService.GetConversationMessages(conversationId, out var errmsg);
            if (messages is null)
                return this.ApiFailedResp(errmsg);
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
