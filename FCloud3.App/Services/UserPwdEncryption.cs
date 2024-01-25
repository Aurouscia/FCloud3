﻿using FCloud3.Utils.Utils.Cryptography;

namespace FCloud3.App.Services
{
    public class UserPwdEncryption
    {
        /// <summary>
        /// 加了盐的md5，投入生产环境后切勿改动盐，否则将造成密码全部失效
        /// </summary>
        /// <param name="password">要加密的密码</param>
        /// <returns>加密后的密码</returns>
        public string Run(string password)
        {
            return MD5Helper.GetMD5Of($"fcloud_{password}");
        }
    }
}
