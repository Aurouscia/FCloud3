using FCloud3.Repos;
using FCloud3.Services;
using FCloud3.Services.Files.Storage;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace FCloud3.App.Services
{
    public static class AppServices
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<HttpUserIdProvider>();
            services.AddScoped<HttpUserInfoService>();
            services.AddControllers(options => {
                options.Filters.Add<ApiExceptionFilter>();
            });
            services.AddMemoryCache();
            services.AddScoped<HtmlGenParserProvider>();
            services.AddScoped<ICommitingUserIdProvider, HttpUserIdProvider>();
            services.AddScoped<IOperatingUserIdProvider, HttpUserIdProvider>();
            services.AddSingleton<IFileStreamHasher, FileStreamHasher>();
            services.AddSingleton<IUserPwdEncryption, UserPwdEncryption>();
            return services;
        }
        public static IServiceCollection AddJwtService(this IServiceCollection services, IConfiguration config)
        {
            string domain = config["Jwt:Domain"] ?? throw new Exception("未找到配置项Jwt:Domain") ;
            string jwtKey = config["Jwt:SecretKey"] ?? throw new Exception("未找到配置项Jwt:SecretKey");

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

        public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration config)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
            services.AddSerilog(logger);
            Log.Logger = logger;
            return services;
        }
    }
}
