using FCloud3.Entities.Identities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Sso.Issuer
{
    /// <summary>
    /// 为其他应用提供基于本系统账号的简易 SSO 签发服务。
    /// 
    /// 配置方式：在 appsettings.json 的 "F3Sso" 节中进行配置：
    /// <code>
    /// "F3Sso": {
    ///   "Enabled": true,
    ///   "Audiences": [
    ///     { "Id": "audienceA", "DisplayName": "应用A", "Origin": "https://appA", "RequireLevel": 1, "Avatar": "" }
    ///   ]
    /// }
    /// </code>
    /// 其中 RequireLevel 对应 <see cref="UserType"/> 的整数值（0=游客，1=会员，8=管理，9=超管）。
    /// </summary>
    public class F3SsoIssuerService
    {
        private readonly IMemoryCache _cache;
        private readonly F3SsoIssuerOptions _options;
        private readonly IUserInfoProvider _userInfoProvider;

        public F3SsoIssuerService(IMemoryCache cache, IConfiguration configuration, IUserInfoProvider userInfoProvider)
        {
            _cache = cache;
            _options = new F3SsoIssuerOptions();
            configuration.GetSection("F3Sso").Bind(_options);
            _userInfoProvider = userInfoProvider;
        }

        public bool CurrentUserMeetsLevel(string audienceId)
        {
            if (!_options.Enabled)
                return false;
            var audience = _options.Audiences?.FirstOrDefault(c => c.Id == audienceId);
            if (audience is null)
                return false;
            return _userInfoProvider.GetUserLevel() >= audience.RequireLevel;
        }

        public string StoreCode()
        {
            var user = new F3SsoValidatedUser
            {
                Id = _userInfoProvider.GetUserId(),
                Name = _userInfoProvider.GetUserName(),
                Type = _userInfoProvider.GetUserLevel()
            };
            var code = Guid.NewGuid().ToString("N");
            _cache.Set(code, user, TimeSpan.FromMinutes(1));
            return code;
        }

        public bool TryGetUser(string code, out F3SsoValidatedUser? user)
        {
            return _cache.TryGetValue(code, out user);
        }

        public F3SsoIssuerOptions GetOptions() => _options;
    }
}
