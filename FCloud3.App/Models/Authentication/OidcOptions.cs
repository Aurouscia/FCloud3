namespace FCloud3.App.Models.Authentication
{
    public class OidcOptions
    {
        public bool Enabled { get; set; } = true;
        public string? ConnStr { get; set; }
        public string? Issuer { get; set; }
        public List<OidcClientOptions> Clients { get; set; } = [];
    }

    public class OidcClientOptions
    {
        public string? ClientId { get; set; }
        public string? DisplayName { get; set; }
        public List<string> RedirectUris { get; set; } = [];
        public List<string> PostLogoutRedirectUris { get; set; } = [];
        public List<string> AllowedScopes { get; set; } = [];
        public List<string> ClientSecrets { get; set; } = [];
        public bool RequirePkce { get; set; } = true;
    }
}
