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

        public bool TryAddMessage(AiMessage message, out string? errmsg)
        {
            TruncateSideEffectFields(message);
            errmsg = null;
            AddAndGetId(message);
            return true;
        }

        private static void TruncateSideEffectFields(AiMessage message)
        {
            if (message.ModelName?.Length > AiMessage.ModelNameMaxLength)
                message.ModelName = message.ModelName[..AiMessage.ModelNameMaxLength];
            if (message.ErrorMessage?.Length > AiMessage.ErrorMessageMaxLength)
                message.ErrorMessage = message.ErrorMessage[..AiMessage.ErrorMessageMaxLength];
            if (message.FinishReason?.Length > AiMessage.FinishReasonMaxLength)
                message.FinishReason = message.FinishReason[..AiMessage.FinishReasonMaxLength];
        }
    }
}
