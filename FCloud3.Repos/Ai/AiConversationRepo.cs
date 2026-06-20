using FCloud3.DbContexts;
using FCloud3.Entities.Ai;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Ai
{
    public class AiConversationRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiConversation>(context, userIdProvider)
    {
        public List<AiConversation> GetByUserAndConfig(int userId, int aiInstanceConfigId)
            => Existing.Where(x => x.UserId == userId && x.AiInstanceConfigId == aiInstanceConfigId)
                       .OrderByDescending(x => x.Updated)
                       .ToList();

        public bool TryAddConversation(AiConversation conversation, out string? errmsg)
        {
            errmsg = Validate(conversation);
            if (errmsg is not null)
                return false;
            AddAndGetId(conversation);
            return true;
        }

        public bool TryUpdateConversation(AiConversation conversation, out string? errmsg)
        {
            errmsg = Validate(conversation);
            if (errmsg is not null)
                return false;
            Update(conversation);
            return true;
        }

        public void RemoveConversation(AiConversation conversation) => Remove(conversation);

        private static string? Validate(AiConversation conversation)
        {
            if (conversation.Title?.Length > AiConversation.TitleMaxLength)
                return $"对话标题长度不能超过 {AiConversation.TitleMaxLength}";
            if (conversation.ModelName?.Length > AiConversation.ModelNameMaxLength)
                return $"模型名长度不能超过 {AiConversation.ModelNameMaxLength}";
            return null;
        }
    }
}
