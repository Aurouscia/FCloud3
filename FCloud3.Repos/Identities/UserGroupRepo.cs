using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;
using System.Text.RegularExpressions;

namespace FCloud3.Repos.Identities
{
    public class UserGroupRepo : RepoBase<UserGroup>
    {
        private const string userGroupNamePattern = @"^[\u4E00-\u9FA5A-Za-z0-9]{1,}$";
        public UserGroupRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public IQueryable<UserGroup> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Name != null && x.Name.Contains(str))
                .OrderBy(x => x.Name!.Length)
                .ThenByDescending(x => x.Updated);
        }
        public override int GetOwnerIdById(int id)
            => Existing.Where(x => x.Id == id).Select(x => x.OwnerUserId).FirstOrDefault();
        public int TryAddAndGetId(UserGroup item, out string? errmsg)
        {
            if (!InfoCheck(item, true, out errmsg))
                return 0;
            return base.AddAndGetId(item);
        }
        public bool TryUpdate(UserGroup item, out string? errmsg)
        {
            if (!InfoCheck(item, false, out errmsg))
                return false;
            base.Update(item);
            return true;
        }
        public void Remove(UserGroup item) => base.Remove(item);
        public bool InfoCheck(UserGroup group, bool creating, out string? errmsg)
        {
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                errmsg = "名称不能为空";
                return false;
            }
            if(group.Name.Length < 2 || group.Name.Length > 10)
            {
                errmsg = "用户组名称长度2-10个字符";
                return false;
            }
            if(!Regex.IsMatch(group.Name, userGroupNamePattern))
            {
                errmsg = "用户组名称只能有汉字，字母，数字";
                return false;
            }
            if(creating)
            {
                if (Existing.Any(x => x.Name == group.Name))
                {
                    errmsg = "存在同名用户组";
                    return false;
                }

            }
            else if(Existing.Any(x=>x.Id!=group.Id && x.Name == group.Name))
            {
                errmsg = "存在同名用户组";
                return false;
            }
            errmsg = null;
            return true;
        }
    }
}
