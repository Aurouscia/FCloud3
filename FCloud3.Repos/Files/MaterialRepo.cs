using FCloud3.DbContexts;
using FCloud3.Entities.Files;

namespace FCloud3.Repos.Files
{
    public class MaterialRepo : RepoBase<Material>
    {
        public MaterialRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {

        }
    }
}
