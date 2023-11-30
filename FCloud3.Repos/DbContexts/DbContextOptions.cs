using FCloud3.Utils.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.DbContexts
{
    public class DbContextOptions
    {
        public string ConnStr { get; }
        public string Type { get; }
        public DbContextOptions(string type, string connStr)
        {
            Type = type;
            ConnStr = connStr;
        }
    }
    public static class DbContextSetup
    {
        public static IServiceCollection AddDb(this IServiceCollection services)
        {
            string dbType = (AppSettings.Db.Type ?? throw new Exception("数据库类型未填")).ToLower();
            string connStr = AppSettings.Db.ConnStr ?? throw new Exception("数据库连接字符串未填");

            DbContextOptions options = new(dbType, connStr);
            services.AddSingleton<DbContextOptions>(options);
            if (dbType == "sqlite")
            {
                services.AddDbContext<FCloudContext, FCloudSqliteContext>();
            }
            else if (dbType == "sqlserver")
            {
                services.AddDbContext<FCloudContext, FCloudSqlServerContext>();
            }
            else
                throw new Exception("不支持的数据库类型，请检查配置的Db:Type项");
            return services;
        }
    }
}
