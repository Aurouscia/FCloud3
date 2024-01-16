using FCloud3.App.Services.Configs;
using FCloud3.Services.Files;
using FCloud3.Utils.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Text;

namespace FCloud3.App.Services
{
    public static class AppServices
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<HttpUserInfoService>();
            services.AddControllers(options => {
                options.Filters.Add<ApiExceptionFilter>();
            });
            services.AddMemoryCache();
            services.AddScoped<HtmlGenParserProvider>();
            services.AddSingleton<IOssConfig, OssConfig>();
            return services;
        }
        public static IServiceCollection AddJwtService(this IServiceCollection services)
        {
            string? domain = AppSettings.Jwt.Domain;
            string? jwtKey = AppSettings.Jwt.SecretKey;
            if (string.IsNullOrEmpty(domain))
                throw new Exception("[Jwt域名未配置]");
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("[JwtKey未配置]");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,//是否验证Issuer
                        ValidateAudience = true,//是否验证Audience
                        ValidateLifetime = true,//是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true,//是否验证SecurityKey
                        ValidAudience = domain,//Audience
                        ValidIssuer = domain,//Issuer，这两项和前面签发jwt的设置一致
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))//拿到SecurityKey
                    };
                });
            return services;
        }

        public static void AddSerilog(this WebApplicationBuilder builder)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            builder.Services.AddSerilog(logger);
            Log.Logger = logger;
        }
    }
}
