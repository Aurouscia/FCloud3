using Microsoft.EntityFrameworkCore;

namespace FCloud3.Entities.Ai
{
    [Index(nameof(UserId), nameof(Created))]
    [Index(nameof(AiInstanceConfigId))]
    [Index(nameof(RelatedWikiItemId))]
    [Index(nameof(ConversationId))]
    public class AiUsageRecord : IDbModel
    {
        public int Id { get; set; }
        /// <summary>调用用户Id</summary>
        public int UserId { get; set; }
        /// <summary>使用的AI实例配置Id</summary>
        public int AiInstanceConfigId { get; set; }
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
        /// <summary>关联的词条Id（0表示无）</summary>
        public int RelatedWikiItemId { get; set; }
        /// <summary>关联的对话Id（可为空）</summary>
        public int? ConversationId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int ModelNameMaxLength = 128;
        public const int PromptSummaryMaxLength = 100;
    }
}
