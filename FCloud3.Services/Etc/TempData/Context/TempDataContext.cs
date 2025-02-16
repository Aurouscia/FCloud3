﻿using FCloud3.DbContexts.Utils;
using FCloud3.Services.Etc.TempData.EditLock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FCloud3.Services.Etc.TempData.Context
{
    public class TempDataContext(DbContextOptions<TempDataContext> options) : DbContext(options)
    {
        public DbSet<ContentEditLock> ContentEditLock { get; set; }
    }
    //add-migration XXXX -Context TempDataContext -OutputDir Etc/TempData/Migrations

    public static class TempDataDbSetup
    {
        public static IServiceCollection AddTempDataContext(this IServiceCollection services, string connStr)
        {
            var path = SqliteConnStrParser.GetDataSource(connStr);
            if (path != ":memory:")
            {
                FileInfo f = new(path);
                if (f.Directory is { } && !f.Directory.Exists)
                    f.Directory.Create();
            }
            services.AddDbContext<TempDataContext>(opt =>
            {
                opt.UseSqlite(connStr);
            });
            return services;
        }
    }
}
