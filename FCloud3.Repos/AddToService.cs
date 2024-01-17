using Microsoft.Extensions.DependencyInjection;
using FCloud3.DbContexts;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Files;

namespace FCloud3.Repos
{
    public static class AddToService
    {
        public static IServiceCollection AddRepos(this IServiceCollection services)
        {
            services.AddDb();

            services.AddScoped<UserRepo>();
            services.AddScoped<WikiItemRepo>();
            services.AddScoped<WikiParaRepo>();
            services.AddScoped<TextSectionRepo>();
            services.AddScoped<FileItemRepo>();
            services.AddScoped<FileDirRepo>();

            return services;
        }
    }
}
