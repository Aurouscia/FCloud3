using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos.Identities
{
    public class UserRepo : RepoBaseCache<User, UserCacheModel>
    {
        public UserRepo(
            FCloudContext context,
            ICommitingUserIdProvider userIdProvider) 
            : base(context, userIdProvider)
        {
        }
        public IQueryable<User> GetByName(string name)
        {
            return Existing.Where(x => x.Name == name);
        }
        public string? GetNameById(int id, int lengthLimit = int.MaxValue)
        {
            var name = Existing.Where(x=>x.Id == id).Select(x=>x.Name).FirstOrDefault();
            if (name == null)
                return null;
            if (name.Length > lengthLimit)
                return name.Substring(0, lengthLimit - 1) + "...";
            return name;
        }
        public int TryAddAndGetId(User item, out string? errmsg)
        {
            errmsg = null;
            if (Existing.Any(x => x.Name == item.Name))
            {
                errmsg = "该用户名已经被占用";
                return 0;
            }
            base.Add(item);
            return item.Id;
        }
        public bool TryUpdate(User item, out string? errmsg)
        {
            errmsg = null;
            if (Existing.Any(x => x.Id != item.Id && x.Name == item.Name))
            {
                errmsg = "该用户名已经被占用";
                return false;
            }
            base.Update(item);
            return true;
        }

        public void SetLastUpdateToNow()
        {
            int uid = _userIdProvider.Get();
            base.UpdateTime(uid);
        }
        public IQueryable<User> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Name != null && x.Name.Contains(str))
                .OrderBy(x => x.Name!.Length)
                .ThenByDescending(x => x.Updated);
        }

        protected override IQueryable<UserCacheModel> ConvertToCacheModel(IQueryable<User> q)
        {
            return q.Select(x => new UserCacheModel(x.Id, x.Updated, x.Name, x.AvatarMaterialId, x.Type));
        }
    }

    public class UserCacheModel(
        int id, DateTime updated, string? name,
        int avatarMaterialId, UserType type)
        : CacheModelBase<User>(id, updated)
    {
        public UserType Type { get; } = type;
        public string? Name { get; } = name;
        public int AvatarMaterialId { get; } = avatarMaterialId;
    }
}
