using Microsoft.EntityFrameworkCore;

namespace FCloud3.Entities.Ai
{
    [Index(nameof(ConversationId), nameof(Order))]
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
        /// <summary>生成本条消息时使用的模型名称</summary>
        public string? ModelName { get; set; }

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
