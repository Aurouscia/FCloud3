using FCloud3.Repos;
using FCloud3.Repos.Etc;
using FCloud3.Services.Etc.TempData.Context;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.WikiPreprocessor.Util;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using Serilog;

namespace FCloud3.Services.Test.TestSupport
{
    public class TestingServiceProvider
    {
        private readonly ServiceProvider _serviceProvider;
        public TestingServiceProvider(int asUser = 2)
        {
            ServiceCollection services = new();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Db:Type"] = "memory",
                    ["TempData:ConnStr"] = "Data Source=:memory:",
                    ["FileStorage:Type"] = "NoNeed"
                }).Build();
            services.AddSingleton(config);

            services.AddRepos(config);

            var stubUserIdProvider = new StubUserIdProvider(asUser);
            var userIdProviderCreate = (IServiceProvider _) => stubUserIdProvider;
            services.AddScoped<IOperatingUserIdProvider, StubUserIdProvider>(
                userIdProviderCreate);
            services.AddScoped<ICommitingUserIdProvider, StubUserIdProvider>(
                userIdProviderCreate);
            services.AddSingleton<StubUserIdProvider>(stubUserIdProvider);
            services.AddScoped<IStorage, FakeStorage>(
                _ => new FakeStorage());
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Debug()
                .CreateLogger();
            services.AddLogging(builder => builder.AddSerilog(logger));
            services.AddSingleton<ILocatorHash, FakeLocatorHash>();

            services.AddSingleton<RestClient>();
            services.AddFCloudServices(config);

            _serviceProvider = services.BuildServiceProvider();
        }
        public T Get<T>()
        {
            return _serviceProvider.GetService<T>() ?? throw new NotImplementedException();
        }

        //TODO:不要临时数据了，全合并到大context，避免多实例不一致
        [Obsolete]
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
