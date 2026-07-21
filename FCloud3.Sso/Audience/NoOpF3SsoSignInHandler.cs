using Microsoft.AspNetCore.Http;

namespace FCloud3.Sso.Audience
{
    /// <summary>
    /// 默认的 SSO 登录处理器，什么都不做，用于保持向后兼容。
    /// </summary>
    public sealed class NoOpF3SsoSignInHandler : IF3SsoSignInHandler
    {
        public Task HandleAsync(HttpContext context, string issuerId, F3SsoValidatedUser user)
        {
            return Task.CompletedTask;
        }
    }
}
