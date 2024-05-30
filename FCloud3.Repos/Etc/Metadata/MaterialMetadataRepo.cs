using FCloud3.Entities.Files;
using FCloud3.Repos.Files;
using FCloud3.Repos.Etc.Metadata.Abstraction;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Etc.Metadata
{
    public class MaterialMetadataRepo(
        MaterialRepo repo,
        ILogger<MetadataRepoBase<MaterialMetaData, Material>> logger) 
        : MetadataRepoBase<MaterialMetaData, Material>(repo, logger)
    {
        public void Create(int id, string name, string pathName)
        {
            MaterialMetaData m = new(id, name, pathName);
            base.Insert(m);
        }
        protected override IQueryable<MaterialMetaData> GetFromDbModel(IQueryable<Material> dbModels)
        {
            return dbModels.Select(x => new MaterialMetaData(x.Id, x.Name, x.StorePathName));
        }
        public string GetPathName(string name)
        {
            return DataListSearch(x => x.Name == name)?.PathName ?? "??";
        }

        private static readonly object LockObj = new();
        protected override object Locker => LockObj;
    }

    public class MaterialMetaData: MetadataBase<Material>
    {
        public string? Name { get; set; }
        public string? PathName { get; set; }
        public MaterialMetaData(int id, string? name, string? pathName)
        {
            Id = id;
            Name = name;
            PathName = pathName;
        }
    }
}
