using FCloud3.Services.Identities;
using Microsoft.Extensions.DependencyInjection;

namespace FCloud3.Services
{
    public static class AddToService
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<UserService>();
            return services;
        }
    }
}
