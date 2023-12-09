using FCloud3.Services.Identities;
using FCloud3.Services.Wiki;
using FCloud3.Services.TextSec;
using Microsoft.Extensions.DependencyInjection;

namespace FCloud3.Services
{
    public static class AddToService
    {
        public static IServiceCollection AddFCloudServices(this IServiceCollection services)
        {
            services.AddScoped<UserService>();
            services.AddScoped<WikiItemService>();
            services.AddScoped<TextSectionService>();
            return services;
        }
    }
}
