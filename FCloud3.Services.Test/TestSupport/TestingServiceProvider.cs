using FCloud3.Repos;
using FCloud3.Repos.Etc;
using FCloud3.Services.Etc.TempData.Context;
using FCloud3.Services.Etc.TempData.EditLock;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Wiki;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FCloud3.Services.Etc;
using FCloud3.Services.Files;

namespace FCloud3.Services.Test.TestSupport
{
    public class TestingServiceProvider
    {
        private readonly ServiceProvider _serviceProvider;
        public TestingServiceProvider()
        {
            ServiceCollection services = new();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Db:Type"] = "memory"
                }).Build();
            services.AddSingleton(config);
            services.AddRepos(config);
            var userIdProviderCreate = (IServiceProvider _) => new StubUserIdProvider(2);
            services.AddScoped<IOperatingUserIdProvider, StubUserIdProvider>(
                userIdProviderCreate);
            services.AddScoped<ICommitingUserIdProvider, StubUserIdProvider>(
                userIdProviderCreate);
            services.AddScoped<IStorage, FakeStorage>(
                _ => new FakeStorage());
            var tctx = CreateTempDataContext();
            services.AddScoped<TempDataContext>(_ => tctx);
            services.AddScoped<ContentEditLockService>();
            services.AddScoped<WikiRefService>();
            services.AddSingleton<ILogger<ContentEditLockService>, FakeLogger<ContentEditLockService>>();
            services.AddScoped<LatestWikiExchangeService>();
            services.AddSingleton<ILogger<LatestWikiExchangeService>, FakeLogger<LatestWikiExchangeService>>();
            services.AddScoped<WikiItemService>();
            services.AddScoped<MyWikisService>();
            services.AddScoped<MaterialService>();
            services.AddScoped<WikiTitleContainService>();
            _serviceProvider = services.BuildServiceProvider();
        }
        public T Get<T>()
        {
            return _serviceProvider.GetService<T>() ?? throw new NotImplementedException();
        }


        //TODO:不要临时数据了，全合并到大context，避免多实例不一致
        public static TempDataContext CreateTempDataContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var contextOptions = new DbContextOptionsBuilder<TempDataContext>()
                .UseSqlite(connection)
                .Options;
            var ctx = new TempDataContext(contextOptions);
            ctx.Database.EnsureCreated();
            return ctx;
        }
    }
}
