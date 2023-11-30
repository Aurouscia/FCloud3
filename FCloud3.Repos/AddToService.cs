using Microsoft.Extensions.DependencyInjection;
using FCloud3.DbContexts;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.Cor;
using FCloud3.Repos.Identities;

namespace FCloud3.Repos
{
    public static class AddToService
    {
        public static IServiceCollection AddRepos(this IServiceCollection services)
        {
            services.AddDb();

            services.AddScoped<UserRepo>();
            services.AddScoped<CorrRepo>();
            services.AddScoped<WikiItemRepo>();
            services.AddScoped<TextSectionRepo>();

            return services;
        }
    }
}
