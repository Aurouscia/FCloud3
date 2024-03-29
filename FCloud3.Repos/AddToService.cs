using Microsoft.Extensions.DependencyInjection;
using FCloud3.DbContexts;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Files;
using Microsoft.Extensions.Configuration;
using FCloud3.Repos.Table;
using FCloud3.Repos.WikiParsing;

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

            return services;
        }
    }
}
