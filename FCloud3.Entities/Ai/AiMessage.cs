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
        /// <summary>输入消息的 Token 数（用于上下文截断和用量统计）</summary>
        public int InputTokenCount { get; set; }
        /// <summary>输出消息的 Token 数（仅 Assistant 消息有值）</summary>
        public int OutputTokenCount { get; set; }
        /// <summary>生成本条消息时使用的模型名称</summary>
        public string? ModelName { get; set; }
        /// <summary>消息状态</summary>
        public AiMessageStatus Status { get; set; }
        /// <summary>状态为 Failed 时的错误信息</summary>
        public string? ErrorMessage { get; set; }
        /// <summary>生成响应耗时（毫秒）</summary>
        public int DurationMs { get; set; }
        /// <summary>AI 停止生成的原因，如 stop / length / tool_calls</summary>
        public string? FinishReason { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int ModelNameMaxLength = 128;
        public const int ErrorMessageMaxLength = 1000;
        public const int FinishReasonMaxLength = 32;
    }

    public enum AiMessageRole
    {
        System = 0,
        User = 1,
        Assistant = 2,
        Tool = 3
    }

    public enum AiMessageStatus
    {
        Pending = 0,
        Sent = 1,
        Received = 2,
        Failed = 3
    }
}
