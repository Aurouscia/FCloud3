using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.Etc.Caching.Abstraction;
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
        public override bool TryAddCheck(User item, out string? errmsg)
        {
            errmsg = null;
            if (Existing.Any(x => x.Name == item.Name))
            {
                errmsg = "该用户名已经被占用";
                return false;
            }
            return true;
        }
        public override bool TryEditCheck(User item, out string? errmsg)
        {
            errmsg = null;
            if (Existing.Any(x => x.Id != item.Id && x.Name == item.Name))
            {
                errmsg = "该用户名已经被占用";
                return false;
            }
            return true;
        }

        public void SetLastUpdateToNow()
        {
            int uid = _userIdProvider.Get();
            Existing.Where(x => x.Id == uid)
                .ExecuteUpdate(call=>call.SetProperty(u => u.Updated, DateTime.Now));
        }
        public void SetUserType(int uid, UserType userType)
        {
            Existing.Where(x => x.Id == uid)
                .ExecuteUpdate(call => call.SetProperty(u => u.Type, userType));
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
        public UserType Type { get; set; } = type;
        public string? Name { get; set; } = name;
        public int AvatarMaterialId { get; set; } = avatarMaterialId;
    }
}
