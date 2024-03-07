using FCloud3.App.Utils;
using FCloud3.Services.Identities;

namespace FCloud3.App.Services.Utils
{
    public class UserPwdEncryption : IUserPwdEncryption
    {
        /// <summary>
        /// 加了盐的md5，投入生产环境后切勿改动盐，否则将造成密码全部失效
        /// </summary>
        /// <param name="password">要加密的密码</param>
        /// <returns>加密后的密码</returns>
        public string Run(string password)
        {
            return $"fcloud_{password}".GetMD5();
        }
    }
}
