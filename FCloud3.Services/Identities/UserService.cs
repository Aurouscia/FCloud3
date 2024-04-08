using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Repos;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Services.Etc;
using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace FCloud3.Services.Identities
{
    public partial class UserService(
        UserRepo repo,
        MaterialRepo materialRepo,
        IUserPwdEncryption userPwdEncryption,
        IStorage storage,
        IOperatingUserIdProvider operatingUserIdProvider,
        CacheExpTokenService cacheExpTokenService,
        IMemoryCache memoryCache)
    {
        private readonly UserRepo _repo = repo;
        private readonly MaterialRepo _materialRepo = materialRepo;
        private readonly IUserPwdEncryption _userPwdEncryption = userPwdEncryption;
        private readonly IStorage _storage = storage;
        private readonly IOperatingUserIdProvider _operatingUserIdProvider = operatingUserIdProvider;
        private readonly CacheExpTokenService _cacheExpTokenService = cacheExpTokenService;
        private readonly IMemoryCache _memoryCache = memoryCache;

        [GeneratedRegex("^[\\u4E00-\\u9FA5A-Za-z0-9_]+$")]
        private static partial Regex UsernameRegex();
        private const string UsernameRuleText = "用户名只能包含汉字、字母、数字、下划线";

        [GeneratedRegex("^[a-zA-Z]\\w{5,17}$")]
        private static partial Regex PwdRegex();
        private const string PasswordRuleText = "密码以字母开头，长度在6~18之间，只能包含字母、数字和下划线";

        public UserComModel? GetById(int id)
        {
            var userAndAvatar = (
                from user in _repo.Existing
                where user.Id == id
                join material in _materialRepo.Existing on user.AvatarMaterialId equals material.Id into userWithAvt
                from mt in userWithAvt.DefaultIfEmpty()
                select new { User = user, Avatar = mt.StorePathName })
                .FirstOrDefault();
            if(userAndAvatar is null)
                return null;
            string avatarUrl = AvatarFullUrl(userAndAvatar.Avatar);
            return UserComModel.ExcludePwd(userAndAvatar.User, avatarUrl);
        }

        public UserComModel? GetByName(string name)
        {
            var userAndAvatar = (
                from user in _repo.Existing
                where user.Name == name
                join material in _materialRepo.Existing on user.AvatarMaterialId equals material.Id into userWithAvt
                from mt in userWithAvt.DefaultIfEmpty()
                select new { User = user, Avatar = mt.StorePathName })
                .FirstOrDefault();
            if (userAndAvatar is null)
                return null;
            string avatarUrl = AvatarFullUrl(userAndAvatar.Avatar);
            return UserComModel.ExcludePwd(userAndAvatar.User, avatarUrl);
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
            var items = lines.ConvertAll(x => new UserIndexItem(x.User, AvatarFullUrl(x.Avatar)));
            return new IndexResult<UserIndexItem>(items, pageIdx, pageCount, totalCount);
        }
        private string AvatarFullUrl(string x) => string.IsNullOrEmpty(x) ? defaultAvatar : _storage.FullUrl(x);

        public User? TryMatchNamePwd(string name,string pwd, out string? errmsg)
        {
            var u = _repo.GetByName(name).FirstOrDefault();
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
            User? u = _repo.GetById(id) ?? throw new Exception("找不到指定ID的用户");

            u.Name = name;
            if (!string.IsNullOrEmpty(pwd))
                u.PwdEncrypted = _userPwdEncryption.Run(pwd);

            if (!_repo.TryEdit(u, out errmsg))
                return false;
            return true;
        }

        public bool ReplaceAvatar(int id, int materialId, out string? errmsg)
        {
            User? u = _repo.GetById(id) ?? throw new Exception("找不到指定ID的用户");
            u.AvatarMaterialId = materialId;
            if (!_repo.TryEdit(u, out errmsg))
                return false;
            return true;
        }

        public void SetLastUpdateToNow()
        {
            _repo.SetLastUpdateToNow();
        }

        public bool SetUserType(int id, UserType targetType, UserType operatingUserType, out string? errmsg)
        {
            var u = _repo.GetById(id);
            if(u is null)
            {
                errmsg = "找不到指定用户";
                return false;
            }
            if(u.Type >= UserType.Admin || targetType >= UserType.Admin)
            {
                if(operatingUserType <= UserType.Admin)
                {
                    errmsg = "管理无权设置管理身份";
                    return false;
                }
            }
            u.Type = targetType;
            if(_cacheExpTokenService.UserTypeInfo.TryGetValue(id, out var expManager))
                expManager.CancelAll();
            return _repo.TryEdit(u, out errmsg);
        }

        private static string UserTypeCacheKey(int id) => $"userTypeOf_{id}";
        public UserType GetUserType(int id)
        {
            if(_memoryCache.TryGetValue<UserType>(UserTypeCacheKey(id), out var userType))
            {
                return userType;
            }
            var expManager = _cacheExpTokenService.UserTypeInfo.GetByKey(id);
            userType = _repo.GetTypeById(id);
            MemoryCacheEntryOptions opts = new();
            opts.AddExpirationToken(expManager.GetCancelChangeToken());
            _memoryCache.Set(UserTypeCacheKey(id), userType, opts);
            return userType;
        }
        public UserType GetUserType()
        {
            var uid = _operatingUserIdProvider.Get();
            return GetUserType(uid);
        }

        public string UserTypeText(UserType type)
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


        /// <summary>
        /// 对应前端：\src\models\identities\user.ts
        /// </summary>
        public class UserComModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Pwd { get; set; }
            public int AvatarMaterialId { get; set; }
            public string? AvatarSrc { get; set; }
            public UserType Type { get; set; }

            public static UserComModel ExcludePwd(User u, string avatarSrc)
            {
                return new UserComModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    AvatarMaterialId = u.AvatarMaterialId,
                    AvatarSrc = avatarSrc,
                    Type = u.Type,
                };
            }
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
