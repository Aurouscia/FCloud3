using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Entities.Sys;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Sys;

namespace FCloud3.Repos.Files
{
    public class MaterialRepo(
        FCloudContext context,
        ICommitingUserIdProvider userIdProvider)
        : RepoBaseCache<Material, MaterialCacheModel>(context, userIdProvider)
    {
        public IQueryable<Material> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Name != null && x.Name.Contains(str))
                .OrderBy(x => x.Name!.Length)
                .ThenByDescending(x => x.Updated);
        }

        public int TryAddAndGetId(Material m, out string? errmsg)
        {
            if (!ModelCheck(m, out errmsg))
                return 0;
            return base.AddAndGetId(m);
        }
        public bool TryUpdateInfo(Material m, out string? errmsg)
        {
            if(!ModelCheck(m, out errmsg))
                return false;
            base.Update(m);
            return true;
        }
        public void UpdateInfoWithoutCheck(Material m) => base.Update(m);
        public new void Remove(Material m) => base.Remove(m);

        protected override IQueryable<MaterialCacheModel> ConvertToCacheModel(IQueryable<Material> q)
        {
            return q.Select(x => new MaterialCacheModel(x.Id, x.Updated, x.Name, x.StorePathName));
        }
        protected override LastUpdateType GetLastUpdateType()
            => LastUpdateType.Material;

        public bool ModelCheck(Material m, out string? errmsg)
        {
            if (string.IsNullOrWhiteSpace(m.Name))
            {
                errmsg = "素材名称不能为空";
                return false;
            }
            if (m.Name.Length < 2)
            {
                errmsg = "素材名称最少两个字符";
                return false;
            }
            if (m.Name.Length > Material.displayNameMaxLength)
            {
                errmsg = $"素材名称最多{Material.displayNameMaxLength}字";
                return false;
            }
            if (m.Desc is not null && m.Desc.Length > Material.descMaxLength)
            {
                errmsg = $"素材描述最多{Material.descMaxLength}字";
                return false;
            }
            if (m.StorePathName is not null && m.StorePathName.Length > Material.storePathNameMaxLength)
            {
                errmsg = $"未知错误：素材存储名过长";
                return false;
            }
            if (Existing.Any(x => x.Name == m.Name))
            {
                errmsg = "已存在同名素材";
                return false;
            }
            errmsg = null;
            return true;
        }
    }

    public class MaterialCacheModel(int id, DateTime updated, string? name, string? pathName) 
        : CacheModelBase<Material>(id, updated)
    {
        public string? Name { get; } = name;
        public string? PathName { get; } = pathName;
    }
}
