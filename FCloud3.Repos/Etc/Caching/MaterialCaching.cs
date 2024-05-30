using FCloud3.Entities.Files;
using FCloud3.Repos.Files;
using Microsoft.Extensions.Logging;
using FCloud3.Repos.Etc.Caching.Abstraction;
using FCloud3.DbContexts;

namespace FCloud3.Repos.Etc.Caching
{
    public class MaterialCaching(
        FCloudContext ctx,
        ILogger<CachingBase<MaterialCachingModel, Material>> logger)
        : CachingBase<MaterialCachingModel, Material>(ctx, logger)
    {
        public void Create(int id, string name, string pathName)
        {
            MaterialCachingModel m = new(id, name, pathName);
            Insert(m);
        }
        protected override IQueryable<MaterialCachingModel> GetFromDbModel(IQueryable<Material> dbModels)
        {
            return dbModels.Select(x => new MaterialCachingModel(x.Id, x.Name, x.StorePathName));
        }
        public string GetPathName(string name)
        {
            return DataListSearch(x => x.Name == name)?.PathName ?? "??";
        }
    }

    public class MaterialCachingModel : CachingModelBase<Material>
    {
        public string? Name { get; set; }
        public string? PathName { get; set; }
        public MaterialCachingModel(int id, string? name, string? pathName)
        {
            Id = id;
            Name = name;
            PathName = pathName;
        }
    }
}
