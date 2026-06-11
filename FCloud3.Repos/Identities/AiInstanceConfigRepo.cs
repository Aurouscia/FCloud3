using FCloud3.DbContexts;
using FCloud3.Entities.Ai;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Identities
{
    public class AiInstanceConfigRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiInstanceConfig>(context, userIdProvider)
    {
        public AiInstanceConfig? GetByGroupId(int groupId)
            => Existing.FirstOrDefault(x => x.GroupId == groupId);

        public void UpdateConfig(AiInstanceConfig config) => Update(config);
        public int AddConfig(AiInstanceConfig config) => AddAndGetId(config);
    }
}
