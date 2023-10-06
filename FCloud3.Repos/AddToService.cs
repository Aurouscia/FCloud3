using Microsoft.Extensions.DependencyInjection;
using FCloud3.Utils.Utils.Settings;
using FCloud3.Repos.DB;
using Microsoft.EntityFrameworkCore;
using FCloud3.Repos.Models.Identities;
using FCloud3.Repos.Models.Wiki;
using FCloud3.Repos.Models.Cor;
using FCloud3.Repos.Models.TextSec;

namespace FCloud3.Repos
{
    public static class AddToService
    {
        public static IServiceCollection AddRepos(this IServiceCollection services, SettingsAccessor<AppSettingsModel> settings)
        {
            var connStr = settings.Get(x => x.ConnectionStrings?.SqlServer);
            _ = connStr ?? throw new Exception("缺少数据库连接字符串");
            services.AddDbContext<FCloudContext>(options =>
                options.UseSqlServer(connStr));
            //注意：DBContext对象 线程不安全

            services.AddScoped<UserRepo>();

            services.AddScoped<CorrRepo>();
            services.AddScoped<WikiItemRepo>();
            services.AddScoped<TextSectionRepo>();

            return services;
        }
    }
}
