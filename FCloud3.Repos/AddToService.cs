using Microsoft.Extensions.DependencyInjection;
using FCloud3.Repos.Models.Identities;
using FCloud3.Repos.Models.Wiki;
using FCloud3.Repos.Models.Cor;
using FCloud3.Repos.Models.TextSec;
using FCloud3.DbContexts;

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
