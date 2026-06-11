namespace FCloud3.Entities.Ai
{
    public class AiConversation : IDbModel
    {
        public int Id { get; set; }
        /// <summary>创建者用户Id</summary>
        public int UserId { get; set; }
        /// <summary>关联的AI实例配置Id</summary>
        public int AiInstanceConfigId { get; set; }
        /// <summary>对话标题（用户可编辑，默认取第一条用户消息前20字）</summary>
        public string? Title { get; set; }
        /// <summary>创建时关联的词条Id（可为空，0表示无）</summary>
        public int CurrentWikiItemId { get; set; }
        /// <summary>消息数缓存</summary>
        public int MessageCount { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int TitleMaxLength = 64;
    }
}
