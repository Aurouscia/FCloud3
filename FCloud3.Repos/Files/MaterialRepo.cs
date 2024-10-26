using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.Etc.Caching.Abstraction;

namespace FCloud3.Repos.Files
{
    public class MaterialRepo : RepoBaseCache<Material, MaterialCacheModel>
    {
        public MaterialRepo(
            FCloudContext context,
            ICommitingUserIdProvider userIdProvider) 
            : base(context, userIdProvider)
        { }

        public IQueryable<Material> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Name != null && x.Name.Contains(str))
                .OrderBy(x => x.Name!.Length)
                .ThenByDescending(x => x.Updated);
        }

        protected override IQueryable<MaterialCacheModel> ConvertToCacheModel(IQueryable<Material> q)
        {
            return q.Select(x => new MaterialCacheModel(x.Id, x.Updated, x.Name, x.StorePathName));
        }
    }

    public class MaterialCacheModel(int id, DateTime updated, string? name, string? pathName) 
        : CacheModelBase<Material>(id, updated)
    {
        public string? Name { get; } = name;
        public string? PathName { get; } = pathName;
    }
}
