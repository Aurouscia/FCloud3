using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FCloud3.DbContexts;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Files;
using FCloud3.Repos.Table;
using FCloud3.Repos.WikiParsing;
using FCloud3.Repos.Diff;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos
{
    public static class AddToService
    {
        public static IServiceCollection AddRepos(this IServiceCollection services, IConfiguration config)
        {
            services.AddDb(config);

            services.AddScoped<UserRepo>();
            services.AddScoped<UserToGroupRepo>();
            services.AddScoped<UserGroupRepo>();
            services.AddScoped<AuthGrantRepo>();

            services.AddScoped<WikiItemRepo>();
            services.AddScoped<WikiToDirRepo>();
            services.AddScoped<WikiParaRepo>();
            services.AddScoped<WikiTitleContainRepo>();

            services.AddScoped<WikiTemplateRepo>();

            services.AddScoped<TextSectionRepo>();
            services.AddScoped<FreeTableRepo>();

            services.AddScoped<FileItemRepo>();
            services.AddScoped<FileDirRepo>();
            services.AddScoped<MaterialRepo>();

            services.AddScoped<CommentRepo>();
            services.AddScoped<NotificationRepo>();
            services.AddScoped<OpRecordRepo>();

            services.AddScoped<DiffContentRepo>();

            services.AddScoped<CreatorIdGetter>();

            return services;
        }
    }
}
