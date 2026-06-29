using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FCloud3.Sso.Issuer
{
    public static class F3SsoIssuerEndpointExtensions
    {
        public static WebApplication MapF3SsoIssuerEndpoints(this WebApplication app)
        {
            app.MapGet("/f3sso/iss", (HttpRequest request, F3SsoIssuerService f3SsoIssuerService) =>
            {
                var audienceId = request.Query["clientId"].ToString();
                if (!f3SsoIssuerService.CurrentUserMeetsLevel(audienceId))
                {
                    return Results.BadRequest("该账号等级不足以登录");
                }
                var code = f3SsoIssuerService.StoreCode();
                return Results.Ok(new { code });
            });
            app.MapGet("/f3sso/iss/config", (F3SsoIssuerService f3SsoIssuerService) =>
            {
                return Results.Ok(f3SsoIssuerService.GetOptions());
            });
            app.MapGet("/f3sso/iss/validate/{code}", (string code, F3SsoIssuerService f3SsoIssuerService) =>
            {
                if (!f3SsoIssuerService.TryGetUser(code, out var user) || user is null)
                {
                    return Results.BadRequest("未查询到该code");
                }
                return Results.Ok(user);
            });
            return app;
        }
    }
}
