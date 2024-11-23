using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FCloud3.Services.Identities;
using FCloud3.Services.Wiki;
using FCloud3.Services.TextSec;
using FCloud3.Services.Files;
using FCloud3.Services.Files.Storage;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Table;
using FCloud3.Services.WikiParsing;
using FCloud3.Services.WikiParsing.Support;
using FCloud3.Services.Etc;
using FCloud3.Services.Diff;
using FCloud3.Services.Etc.Split;
using FCloud3.Services.Etc.TempData.Context;
using FCloud3.Services.Etc.TempData.EditLock;
using FCloud3.Services.Messages;
using FCloud3.Services.Wiki.Support;
using FCloud3.Services.Etc.Cache;

namespace FCloud3.Services
{
    public static class AddToService
    {
        public static IServiceCollection AddFCloudServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<UserService>();
            services.AddScoped<UserGroupService>();
            services.AddScoped<AuthGrantService>();

            services.AddScoped<WikiItemService>();
            services.AddScoped<WikiParaService>();
            services.AddScoped<WikiTitleContainService>();
            services.AddScoped<WikiRefService>();
            services.AddScoped<TextSectionService>();
            services.AddScoped<FreeTableService>();
            services.AddScoped<FileItemService>();
            services.AddScoped<FileDirService>();
            services.AddScoped<MaterialService>();
            services.AddScoped<DiffContentService>();
            services.AddScoped<WikiSelectedService>();

            services.AddScoped<WikiParserProviderService>();
            services.AddScoped<WikiParserDataSource>();
            services.AddScoped<WikiParsingRulesProviderService>();
            services.AddScoped<WikiTemplateService>();
            services.AddScoped<WikiParsingService>();
            services.AddSingleton<WikiParsedResultService>();
            services.AddScoped<WikiRecommendService>();

            services.AddScoped<CommentService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<OpRecordService>();

            services.AddScoped<QuickSearchService>();
            services.AddScoped<LatestWorkService>();
            services.AddScoped<WikiCenteredHomePageService>();
            services.AddScoped<WikiContentSearchService>();
            services.AddScoped<MyWikisService>();
            services.AddScoped<LatestWikiExchangeService>();
            services.AddScoped<SplitService>();

            services.AddScoped<AuthResCacheHost>();
            services.AddScoped<WikiParserCacheHost>();

            string storageType = config["FileStorage:Type"] ?? "Local";
            if (storageType == "Local")
                services.AddSingleton<IStorage, LocalStorage>();
            else if (storageType == "Oss")
                services.AddSingleton<IStorage, OssStorage>();
            else if (storageType == "NoNeed")
            {

            }
            else
                throw new Exception("不支持的文件存储类型(配置项FileStorage:Type)");



            string tempDataConnStr = config["TempData:ConnStr"] 
                ?? throw new Exception("临时数据配置未填写");
            services.AddTempDataContext(tempDataConnStr);
            services.AddScoped<ContentEditLockService>();
            return services;
        }
    }
}
