using FCloud3.DbContexts;
using FCloud3.Entities.Ai;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Ai
{
    public class AiUsageRecordRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiUsageRecord>(context, userIdProvider)
    {
        public int AddRecord(AiUsageRecord record) => AddAndGetId(record);
    }
}
