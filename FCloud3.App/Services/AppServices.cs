using FCloud3.Utils.Utils.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FCloud3.App.Services
{
    public static class AppServices
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<HttpUserInfoService>();
            return services;
        }
        public static IServiceCollection AddJwtService(this IServiceCollection services, SettingsAccessor<AppSettingsModel> settings)
        {
            string domain = settings.Get(x => x.SiteInfo?.Domain) ?? "域名未配置";
            string jwtKey = settings.Get(x => x.SiteInfo?.JwtSecretKey) ?? "jwtKey未配置";
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
    }
}
