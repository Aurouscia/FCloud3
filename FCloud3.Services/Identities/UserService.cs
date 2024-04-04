using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Repos;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Services.Files.Storage.Abstractions;
using System.Text.RegularExpressions;

namespace FCloud3.Services.Identities
{
    public partial class UserService
    {
        private readonly UserRepo _repo;
        private readonly MaterialRepo _materialRepo;
        private readonly IUserPwdEncryption _userPwdEncryption;
        private readonly IStorage _storage;

        public UserService(UserRepo repo, MaterialRepo materialRepo, IUserPwdEncryption userPwdEncryption, IStorage storage)
        {
            _repo = repo;
            _materialRepo = materialRepo;
            _userPwdEncryption = userPwdEncryption;
            _storage = storage;
        }

        [GeneratedRegex("^[\\u4E00-\\u9FA5A-Za-z0-9_]+$")]
        private static partial Regex UsernameRegex();
        private const string UsernameRuleText = "用户名只能包含汉字、字母、数字、下划线";

        [GeneratedRegex("^[a-zA-Z]\\w{5,17}$")]
        private static partial Regex PwdRegex();
        private const string PasswordRuleText = "密码以字母开头，长度在6~18之间，只能包含字母、数字和下划线";

        public User? GetById(int id)
        {
            return _repo.GetById(id);
        }

        private const string defaultAvatar = "/defaultAvatar.svg";
        public IndexResult<UserIndexItem> Index(IndexQuery q)
        {
            static string keyReplaceForLastOperation(string k)
            {
                if (k == nameof(UserIndexItem.LastOperation))
                    return nameof(User.Updated);
                return k;
            }
            var pagedUser = _repo.IndexFilterOrder(q, keyReplaceForLastOperation).TakePage(q, out int totalCount, out int pageIdx, out int pageCount);
            var lines = (
                from user in pagedUser
                join material in _materialRepo.Existing on user.AvatarMaterialId equals material.Id into userWithAvt
                from mt in userWithAvt.DefaultIfEmpty()
                select new { 
                    User = user, 
                    Avatar = mt.StorePathName }).ToList();
            Func<string?, string?> avatarFullUrl = x => string.IsNullOrEmpty(x) ? defaultAvatar : _storage.FullUrl(x);
            var items = lines.ConvertAll(x => new UserIndexItem(x.User, avatarFullUrl(x.Avatar)));
            return new IndexResult<UserIndexItem>(items, pageIdx, pageCount, totalCount);
        }

        public User? GetByName(string name)
        {
            return _repo.Existing.Where(x => x.Name == name).FirstOrDefault();
        }

        public User? TryMatchNamePwd(string name,string pwd, out string? errmsg)
        {
            var u = GetByName(name);
            if(u is null)
            {
                errmsg = "用户名不存在";
                return null;
            }    
            if(u.PwdEncrypted != _userPwdEncryption.Run(pwd))
            {
                errmsg = "密码错误";
                return null;
            }
            errmsg = null;
            return u;
        }

        public static bool BasicInfoCheck(string? name,string pwd,out string? errmsg,bool allowEmptyPwd=false)
        {
            errmsg = null;
            if (string.IsNullOrEmpty(name))
            {
                errmsg = "用户名不能为空"; return false;
            }
            if (!allowEmptyPwd && string.IsNullOrEmpty(pwd))
            {
                errmsg = "密码不能为空"; return false;
            }
            if (!UsernameRegex().IsMatch(name))
            {
                errmsg = UsernameRuleText; return false;
            }
            if (!allowEmptyPwd && !PwdRegex().IsMatch(pwd))
            {
                errmsg = PasswordRuleText; return false;
            }
            return true;
        }

        public bool TryCreate(string? name,string? pwd,out string? errmsg)
        {
            pwd ??= "";
            if(!BasicInfoCheck(name,pwd,out errmsg))
                return false;

            User u = new()
            {
                Name = name,
                PwdEncrypted = _userPwdEncryption.Run(pwd)
            };

            if (!_repo.TryAdd(u,out errmsg))
                return false;
            return true;
        }

        public bool TryEdit(int id, string? name,string? pwd,out string? errmsg)
        {
            pwd ??= "";
            if (!BasicInfoCheck(name, pwd, out errmsg, allowEmptyPwd:true))
                return false;
            User? u = GetById(id) ?? throw new Exception("找不到指定ID的用户");

            u.Name = name;
            if (!string.IsNullOrEmpty(pwd))
                u.PwdEncrypted = _userPwdEncryption.Run(pwd);

            if (!_repo.TryEdit(u, out errmsg))
                return false;
            return true;
        }

        public void SetLastUpdateToNow()
        {
            _repo.SetLastUpdateToNow();
        }

        public static string UserTypeText(UserType type)
        {
            if(type == UserType.Tourist)
                return "游客";
            if(type == UserType.Member)
                return "成员";
            if(type == UserType.Admin)
                return "管理";
            if(type == UserType.SuperAdmin)
                return "超管";
            return "未知";
        }

        public class UserIndexItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LastOperation { get; set; }
            public string? Avatar { get; set; }
            public UserType Type { get; set; }
            public UserIndexItem(User u, string? avatar)
            {
                Id = u.Id;
                Name = u.Name ?? "??";
                LastOperation = u.Updated.ToString("yyyy/MM/dd HH:mm");
                Avatar = avatar;
                Type = u.Type;
            }
        }
    }

    public interface IUserPwdEncryption
    {
        public string Run(string password);
    }
}
