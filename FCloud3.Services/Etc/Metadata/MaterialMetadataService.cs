using FCloud3.Entities.Files;
using FCloud3.Repos.Files;
using FCloud3.Services.Etc.Metadata.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Etc.Metadata
{
    public class MaterialMetadataService(MaterialRepo repo) : MetadataServiceBase<MaterialMetaData, Material>(repo)
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
            return DataList.Where(x => x.Name == name).Select(x => x.PathName).FirstOrDefault() ?? "??";
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
