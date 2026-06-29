using Microsoft.AspNetCore.Http;

namespace FCloud3.Sso.Audience
{
    /// <summary>
    /// SSO 验证成功后，由宿主应用实现的用户登录处理接口。
    /// </summary>
    public interface IF3SsoSignInHandler
    {
        /// <summary>
        /// 处理验证成功的用户信息，例如写入 Cookie、签发 JWT 或创建 Session。
        /// </summary>
        /// <param name="context">当前 HTTP 上下文</param>
        /// <param name="issuerId">签发方标识，表明用户来自哪个 issuer</param>
        /// <param name="user">验证成功的用户信息</param>
        Task HandleAsync(HttpContext context, string issuerId, F3SsoValidatedUser user);
    }
}
