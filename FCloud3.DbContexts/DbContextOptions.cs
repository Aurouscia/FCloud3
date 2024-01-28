using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.DbContexts
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
        public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection("Db");
            string dbType = section["Type"] ?? throw new Exception("Db:Type未填");
            string connStr = section["ConnStr"] ?? throw new Exception("Db:ConnStr未填");
            dbType = dbType.ToLower();

            DbContextOptions options = new(dbType, connStr);
            services.AddSingleton<DbContextOptions>(options);

            if (dbType == "sqlite")
            {
                services.AddDbContext<FCloudContext, FCloudSqliteContext>();
            }
            else if (dbType == "sqlitedev")
            {
                services.AddDbContext<FCloudContext, FCloudSqliteDevContext>();
            }
            else if (dbType == "sqlserver")
            {
                services.AddDbContext<FCloudContext, FCloudSqlServerContext>();
            }
            else
                throw new Exception("不支持的数据库类型(配置项Db:Type)");

            services.AddScoped<DbTransactionService>();

            return services;
        }
    }
}
