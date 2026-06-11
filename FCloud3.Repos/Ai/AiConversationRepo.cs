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

        public void UpdateConversation(AiConversation conversation) => Update(conversation);
        public int AddConversation(AiConversation conversation) => AddAndGetId(conversation);
        public void RemoveConversation(AiConversation conversation) => Remove(conversation);
    }
}
