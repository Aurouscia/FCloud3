using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.Repos.Identities
{
    public class UserGroupRepo : RepoBase<UserGroup>
    {
        private const string userGroupNamePattern = @"^[\u4E00-\u9FA5A-Za-z0-9]{1,}$";
        public UserGroupRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }
        public override bool TryAddCheck(UserGroup item, out string? errmsg)
        {
            return InfoCheck(item, out errmsg);
        }
        public override bool TryEditCheck(UserGroup item, out string? errmsg)
        {
            return InfoCheck(item, out errmsg);
        }
        public bool InfoCheck(UserGroup group, out string? errmsg)
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
            if(Existing.Any(x=>x.Id!=group.Id && x.Name == group.Name))
            {
                errmsg = "存在同名用户组";
                return false;
            }
            errmsg = null;
            return true;
        }
    }
}
