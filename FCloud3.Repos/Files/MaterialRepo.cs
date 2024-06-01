using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Etc.Caching;

namespace FCloud3.Repos.Files
{
    public class MaterialRepo : RepoBaseWithCaching<Material, MaterialCachingModel>
    {
        public MaterialRepo(
            FCloudContext context,
            ICommitingUserIdProvider userIdProvider,
            MaterialCaching materialCaching) 
            : base(context, userIdProvider, materialCaching)
        { }

        public IQueryable<Material> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Name != null && x.Name.Contains(str))
                .OrderBy(x => x.Name!.Length)
                .ThenByDescending(x => x.Updated);
        }
    }
}
