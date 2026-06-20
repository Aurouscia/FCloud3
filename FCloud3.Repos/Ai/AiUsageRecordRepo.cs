using FCloud3.DbContexts;
using FCloud3.Entities.Ai;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Ai
{
    public class AiUsageRecordRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiUsageRecord>(context, userIdProvider)
    {
        public bool TryAddRecord(AiUsageRecord record, out string? errmsg)
        {
            TruncateSideEffectFields(record);
            errmsg = null;
            AddAndGetId(record);
            return true;
        }

        private static void TruncateSideEffectFields(AiUsageRecord record)
        {
            if (record.ModelName?.Length > AiUsageRecord.ModelNameMaxLength)
                record.ModelName = record.ModelName[..AiUsageRecord.ModelNameMaxLength];
            if (record.PromptSummary?.Length > AiUsageRecord.PromptSummaryMaxLength)
                record.PromptSummary = record.PromptSummary[..AiUsageRecord.PromptSummaryMaxLength];
        }
    }
}
