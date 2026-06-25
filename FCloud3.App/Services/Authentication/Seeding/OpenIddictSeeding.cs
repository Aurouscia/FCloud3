using FCloud3.App.Models.Authentication;
using OpenIddict.Abstractions;

namespace FCloud3.App.Services.Authentication.Seeding
{
    public static class OpenIddictSeeding
    {
        public static async Task SeedApplicationsAsync(IServiceProvider serviceProvider)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var oidcOptions = new OidcOptions();
            config.GetSection("Oidc").Bind(oidcOptions);

            foreach (var client in oidcOptions.Clients)
            {
                if (string.IsNullOrWhiteSpace(client.ClientId))
                    continue;

                var existing = await manager.FindByClientIdAsync(client.ClientId);
                if (existing is not null)
                    continue;

                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = client.ClientId,
                    DisplayName = client.DisplayName,
                    RedirectUris = { },
                    PostLogoutRedirectUris = { },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.EndSession,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Profile
                    }
                };

                foreach (var uri in client.RedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
                    descriptor.RedirectUris.Add(new Uri(uri));

                foreach (var uri in client.PostLogoutRedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
                    descriptor.PostLogoutRedirectUris.Add(new Uri(uri));

                foreach (var allowedScope in client.AllowedScopes.Where(s => !string.IsNullOrWhiteSpace(s)))
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + allowedScope);

                if (client.RequirePkce)
                    descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

                if (client.ClientSecrets.Any(s => !string.IsNullOrWhiteSpace(s)))
                {
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);
                }

                // 简单处理：将第一个非空 secret 写入；多个 secret 需额外处理
                var firstSecret = client.ClientSecrets.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                if (!string.IsNullOrWhiteSpace(firstSecret))
                {
                    descriptor.ClientSecret = firstSecret;
                }

                await manager.CreateAsync(descriptor);
            }
        }
    }
}
