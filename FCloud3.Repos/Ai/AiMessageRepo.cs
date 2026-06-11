using FCloud3.DbContexts;
using FCloud3.Entities.Ai;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Ai
{
    public class AiMessageRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiMessage>(context, userIdProvider)
    {
        public List<AiMessage> GetByConversationId(int conversationId)
            => Existing.Where(x => x.ConversationId == conversationId)
                       .OrderBy(x => x.Order)
                       .ToList();

        public int GetMaxOrder(int conversationId)
            => Existing.Where(x => x.ConversationId == conversationId)
                       .Max(x => (int?)x.Order) ?? 0;

        public int AddMessage(AiMessage message) => AddAndGetId(message);
    }
}
