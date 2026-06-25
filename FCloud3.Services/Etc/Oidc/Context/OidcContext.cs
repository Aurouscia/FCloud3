using FCloud3.DbContexts.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FCloud3.Services.Etc.Oidc.Context
{
    public class OidcContext(DbContextOptions<OidcContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseOpenIddict();
        }
    }
    //add-migration InitOidc -Context OidcContext -Project FCloud3.Services -StartupProject FCloud3.App -OutputDir Etc/Oidc/Migrations

    public static class OidcDbSetup
    {
        public static IServiceCollection AddOidcContext(this IServiceCollection services, string connStr)
        {
            var path = SqliteConnStrParser.GetDataSource(connStr);
            if (path != ":memory:")
            {
                FileInfo f = new(path);
                if (f.Directory is { } && !f.Directory.Exists)
                    f.Directory.Create();
            }
            services.AddDbContext<OidcContext>(opt =>
            {
                opt.UseSqlite(connStr);
            });
            return services;
        }
    }
}
