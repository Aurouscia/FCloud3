using FCloud3.Entities.Files;
using FCloud3.Repos.Files;
using FCloud3.Services.Etc.Metadata.Abstraction;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Etc.Metadata
{
    public class MaterialMetadataService(
        MaterialRepo repo,
        ILogger<MetadataServiceBase<MaterialMetaData, Material>> logger) 
        : MetadataServiceBase<MaterialMetaData, Material>(repo, logger)
    {
        public void Create(int id, string name, string pathName)
        {
            MaterialMetaData m = new(id, name, pathName);
            base.Create(m);
        }
        protected override IQueryable<MaterialMetaData> GetFromDbModel(IQueryable<Material> dbModels)
        {
            return dbModels.Select(x => new MaterialMetaData(x.Id, x.Name, x.StorePathName));
        }
        public string GetPathName(string name)
        {
            return DataListSearch(x => x.Name == name)?.PathName ?? "??";
        }
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
