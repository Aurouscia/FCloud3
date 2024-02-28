using Microsoft.AspNetCore.Authorization;

namespace FCloud3.App.Services.Auth
{
    public static class AuthService
    {
        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            //services.AddAuthorization(options =>
            //{
            //    options.AddAuthGrants();
            //});
            //services.AddScoped<IAuthorizationHandler, AuthGrantsHandler>();
            //services.AddScoped<AuthGrantsOnIdProvider>();
            return services;
        }
    }
}
