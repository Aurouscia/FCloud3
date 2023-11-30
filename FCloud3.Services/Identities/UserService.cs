using FCloud3.Entities.DbModels.Identities;
using FCloud3.Repos.Models.Identities;
using FCloud3.Utils.Utils.Cryptography;
using System.Text.RegularExpressions;

namespace FCloud3.Services.Identities
{
    public partial class UserService
    {
        private readonly UserRepo _repo;
        public UserService(UserRepo repo)
        {
            _repo = repo;
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
                PwdMd5 = MD5Helper.GetMD5Of(pwd)
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
                u.PwdMd5 = MD5Helper.GetMD5Of(pwd);

            if (!_repo.TryEdit(u, out errmsg))
                return false;
            return true;
        }
    }
}
