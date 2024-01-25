using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using FCloud3.Services.Identities;
using FCloud3.App.Services;
using FCloud3.Repos.Identities;
using FCloud3.Utils.Settings;
using FCloud3.Entities.Identities;
using FCloud3.Utils.Utils.Cryptography;

namespace FCloud3.App.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserService _userService;
        private readonly UserPwdEncryption _userPwdEncryption;
        private readonly HttpUserInfoService _userInfo;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserService userService,
            UserPwdEncryption userPwdEncryption,
            HttpUserInfoService userInfo,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _userPwdEncryption = userPwdEncryption;
            _userInfo = userInfo;
            _logger = logger;
        }
        public IActionResult Login(string? userName, string? password)
        {
            _logger.LogInformation("登录请求：{userName}",userName);

            if (userName is null || password is null)
                return this.ApiFailedResp("请填写用户名和密码");
            var u = _userService.GetByName(userName);
            if (u is null)
                return this.ApiFailedResp("不存在的用户名");
            string pwdmd5 = _userPwdEncryption.Run(password);
            if (u.PwdEncrypted != pwdmd5)
                return this.ApiFailedResp("密码错误");

            string domain = AppSettings.Jwt.Domain ?? throw new Exception("缺少配置[Jwt:Domain]");
            string secret = AppSettings.Jwt.SecretKey ?? throw new Exception("缺少配置[Jwt:Secret]");
            int expHours = 72;
            var claims = new[]
            {
                    new Claim (JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                    new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddHours(expHours)).ToUnixTimeSeconds()}"),
                    new Claim (JwtRegisteredClaimNames.NameId, u.Id.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: domain,
                audience: domain,
                claims: claims,
                expires: DateTime.Now.AddHours(expHours),
                signingCredentials: creds
            );

            string tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("{userName}登录成功",userName);
            return this.ApiResp(new { token = tokenStr });
        }
        [Authorize]
        public IActionResult IdentityTest()
        {
            return this.ApiResp(_userInfo);
        }
    }
}
