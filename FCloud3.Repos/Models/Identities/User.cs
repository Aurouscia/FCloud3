using FCloud3.Repos.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Identities
{
    public class User : IDbModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PwdMd5 { get; set; }
        public string? AvatarFileName { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }

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
    }
}
