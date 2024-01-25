using FCloud3.DbContexts;
using FCloud3.Entities.Identities;

namespace FCloud3.Repos.Identities
{
    public class UserRepo : RepoBase<User>
    {
        public UserRepo(FCloudContext context) : base(context)
        {

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
        public IQueryable<User> QuickSearch(string str)
        {
            return Existing.Where(x => x.Name != null && x.Name.Contains(str));
        }
    }
}
