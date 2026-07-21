using System.Text.Json;
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
        public async Task GetSuggestions(
            int aiInstanceConfigId, string prompt, int? conversationId, int currentWikiItemId,
            CancellationToken ct)
        {
            Response.ContentType = "application/x-ndjson; charset=utf-8";
            await foreach (var chunk in aiChatService.GetSuggestions(
                aiInstanceConfigId, prompt, conversationId, currentWikiItemId, ct))
            {
                var json = JsonSerializer.Serialize(chunk) + "\n";
                await Response.WriteAsync(json, ct);
                await Response.Body.FlushAsync(ct);
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
        public IActionResult GetConversations(int aiInstanceConfigId, bool includeArchived = false)
        {
            var list = aiChatService.GetConversations(aiInstanceConfigId, includeArchived, out var errmsg);
            if (list is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(list);
        }

        /// <summary>设置对话顶置状态</summary>
        public IActionResult SetConversationPinned(int conversationId, bool isPinned)
        {
            var res = aiChatService.SetConversationPinned(conversationId, isPinned, out var errmsg);
            if (!res) return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        /// <summary>设置对话归档状态</summary>
        public IActionResult SetConversationArchived(int conversationId, bool isArchived)
        {
            var res = aiChatService.SetConversationArchived(conversationId, isArchived, out var errmsg);
            if (!res) return this.ApiFailedResp(errmsg);
            return this.ApiResp();
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
