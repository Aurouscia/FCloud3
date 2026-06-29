using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Sso.Audience
{
    public static class F3SsoAudienceEndpointExtensions
    {
        public static WebApplication MapF3SsoAudienceEndpoints(this WebApplication app)
        {
            app.MapGet("/f3sso/aud/config", (IConfiguration configuration) =>
            {
                var options = new F3SsoAudienceOptions();
                configuration.GetSection("F3Sso").Bind(options);
                return Results.Ok(options);
            });

            app.MapGet("/f3sso/aud/validate", async (HttpRequest request, IConfiguration configuration) =>
            {
                var code = request.Query["code"].ToString();
                var issuerId = request.Query["issuerId"].ToString();

                var options = new F3SsoAudienceOptions();
                configuration.GetSection("F3Sso").Bind(options);

                string? errmsg = null;
                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(issuerId))
                {
                    errmsg = "缺少 code 或 issuerId";
                }
                else
                {
                    var audience = new F3SsoAudience(configuration);
                    var user = await audience.ValidateCodeAsync(issuerId, code);
                    if (user is null)
                    {
                        errmsg = "验证失败";
                    }
                }

                var redirectUrl = BuildRedirectUrl(options.RedirectPath, errmsg);
                return Results.Redirect(redirectUrl);
            });

            string BuildRedirectUrl(string redirectPath, string? errorMessage)
            {
                var success = errorMessage is null ? "1" : "0";
                var url = $"{redirectPath}?success={success}";
                if (errorMessage is not null)
                {
                    url += $"&errmsg={Uri.EscapeDataString(errorMessage)}";
                }
                return url;
            }

            return app;
        }
    }
}
