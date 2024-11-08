using FCloud3.DbContexts.DbSpecific;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCloud3.DbContexts
{
    public class FCloudContextOptions
    {
        public string? ConnStr { get; set; }
        public string? Type { get; set; }
        public SqlServerOptions? SqlServer { get; set; }

        public class SqlServerOptions
        {
            public int CompatibilityLevel { get; set; }
        }
    }


    public static class DbContextSetup
    {
        public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection("Db");
            var options = new FCloudContextOptions();
            section.Bind(options);
            services.AddSingleton<FCloudContextOptions>(options);

            string dbType = options.Type?.ToLower() ?? throw new Exception("数据库类型(配置项Db:Type)未填");

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
            else if (dbType == "memory")
            {
                services.AddScoped<FCloudContext>((_) => FCloudMemoryContext.Create());
            }
            else
                throw new Exception("不支持的数据库类型(配置项Db:Type)");

            services.AddScoped<DbTransactionService>();

            return services;
        }
    }
}
