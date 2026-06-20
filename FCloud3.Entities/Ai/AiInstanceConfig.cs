using Microsoft.EntityFrameworkCore;

namespace FCloud3.Entities.Ai
{
    [Index(nameof(GroupId))]
    [Index(nameof(DefaultDirId))]
    public class AiInstanceConfig : IDbModel
    {
        public int Id { get; set; }
        /// <summary>关联的团体Id</summary>
        public int GroupId { get; set; }
        /// <summary>实例名称，用于在列表中区分不同实例</summary>
        public string? InstanceName { get; set; }
        /// <summary>API 基础地址，如 https://api.openai.com/v1</summary>
        public string? ApiBaseUrl { get; set; }
        /// <summary>API Key（加密存储）</summary>
        public string? ApiKey { get; set; }
        /// <summary>默认模型名称，如 gpt-4o。用户可在创建对话时选择其他模型覆盖。</summary>
        public string? DefaultModelName { get; set; }
        /// <summary>系统提示词</summary>
        public string? SystemPrompt { get; set; }
        /// <summary>是否启用 AI 功能</summary>
        public bool Enabled { get; set; }
        /// <summary>默认 AI 可查看的目录范围（某目录及其子级），0 表示不限</summary>
        public int DefaultDirId { get; set; }
        /// <summary>最大上下文消息数（0表示不限制）</summary>
        public int MaxContextMessages { get; set; }
        /// <summary>每日Token限额（0表示不限）</summary>
        public int DailyTokenLimit { get; set; }
        /// <summary>每月Token限额（0表示不限）</summary>
        public int MonthlyTokenLimit { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
