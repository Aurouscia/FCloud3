using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    /// 其中 RequireLevel 的具体含义由使用方应用自行约定。
    /// </summary>
    public class F3SsoIssuerService
    {
        private readonly IMemoryCache _cache;
        private readonly F3SsoIssuerOptions _options;
        private readonly IUserInfoProvider _userInfoProvider;
        private readonly ILogger<F3SsoIssuerService> _logger;

        public F3SsoIssuerService(
            IMemoryCache cache,
            IConfiguration configuration,
            IUserInfoProvider userInfoProvider,
            ILogger<F3SsoIssuerService> logger)
        {
            _cache = cache;
            _options = new F3SsoIssuerOptions();
            configuration.GetSection("F3Sso").Bind(_options);
            _userInfoProvider = userInfoProvider;
            _logger = logger;
        }

        public bool CurrentUserMeetsLevel(string audienceId)
        {
            if (!_options.Enabled)
            {
                _logger.LogWarning("SSO 未启用，拒绝 audience {AudienceId} 的登录请求", audienceId);
                return false;
            }
            var audience = _options.Audiences?.FirstOrDefault(c => c.Id == audienceId);
            if (audience is null)
            {
                _logger.LogWarning("未找到 audience {AudienceId}，拒绝登录请求", audienceId);
                return false;
            }
            var userLevel = _userInfoProvider.GetUserLevel();
            if (userLevel < audience.RequireLevel)
            {
                _logger.LogWarning(
                    "用户 {UserId} 等级 {UserLevel} 不足，无法登录 audience {AudienceId}（要求 {RequireLevel}）",
                    _userInfoProvider.GetUserId(), userLevel, audienceId, audience.RequireLevel);
                return false;
            }
            _logger.LogDebug("用户 {UserId} 允许登录 audience {AudienceId}", _userInfoProvider.GetUserId(), audienceId);
            return true;
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
            _logger.LogDebug("为用户 {UserId} 生成 SSO code", user.Id);
            return code;
        }

        public bool TryGetUser(string code, out F3SsoValidatedUser? user)
        {
            if (_cache.TryGetValue(code, out user) && user is not null)
            {
                _logger.LogDebug("验证 SSO code 成功，用户 {UserId}", user.Id);
                return true;
            }
            _logger.LogWarning("验证不存在的 SSO code");
            user = null;
            return false;
        }

        public F3SsoIssuerOptions GetOptions() => _options;
    }
}
