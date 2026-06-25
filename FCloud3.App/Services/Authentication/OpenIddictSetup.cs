using FCloud3.Services.Etc.Oidc.Context;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using System.Text;

namespace FCloud3.App.Services.Authentication
{
    public static class OpenIddictSetup
    {
        public static IServiceCollection AddOpenIddictServerWithOidc(this IServiceCollection services, IConfiguration config)
        {
            bool enabled = config.GetValue<bool?>("Oidc:Enabled") ?? true;
            if (!enabled)
                return services;

            string issuer = config["Oidc:Issuer"]
                ?? throw new Exception("未找到配置项Oidc:Issuer");
            string jwtKey = config["Jwt:SecretKey"]
                ?? throw new Exception("未找到配置项Jwt:SecretKey");

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<OidcContext>();
                })
                .AddServer(options =>
                {
                    options.SetIssuer(new Uri(issuer));

                    options.SetAuthorizationEndpointUris("connect/authorize")
                           .SetTokenEndpointUris("connect/token")
                           .SetUserInfoEndpointUris("connect/userinfo")
                           .SetEndSessionEndpointUris("connect/logout");

                    options.AllowAuthorizationCodeFlow();
                    options.RequireProofKeyForCodeExchange();

                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Scopes.Profile);

                    options.RegisterClaims(
                        OpenIddictConstants.Claims.Name,
                        OpenIddictConstants.Claims.PreferredUsername);

                    // 使用与现有 JWT 相同的对称密钥签名，便于资源服务器复用验证
                    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                    options.AddSigningKey(signingKey);

                    // 开发加密证书；生产环境应替换为持久化证书
                    options.AddDevelopmentEncryptionCertificate();

                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .EnableUserInfoEndpointPassthrough()
                           .EnableEndSessionEndpointPassthrough()
                           .EnableStatusCodePagesIntegration();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            return services;
        }
    }
}
