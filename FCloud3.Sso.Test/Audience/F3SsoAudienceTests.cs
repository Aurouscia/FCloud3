using FCloud3.Sso.Audience;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Sso.Test.Audience
{
    public class F3SsoAudienceTests
    {
        private static F3SsoAudience CreateAudience(
            string audienceId = "appa",
            string? issuerClientId = "appA")
        {
            var values = new Dictionary<string, string?>
            {
                ["F3Sso:Id"] = audienceId,
                ["F3Sso:RedirectPath"] = "/sso/callback",
                ["F3Sso:Issuers:0:Id"] = "main-issuer",
                ["F3Sso:Issuers:0:Origin"] = "https://fcloud3.example.com"
            };
            if (issuerClientId is not null)
            {
                values["F3Sso:Issuers:0:ClientId"] = issuerClientId;
            }

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
            return new F3SsoAudience(config);
        }

        [Fact]
        public void BuildAuthorizeUrl_WithExplicitClientId_ShouldUseClientId()
        {
            var audience = CreateAudience(audienceId: "appa", issuerClientId: "registered-appA");

            var url = audience.BuildAuthorizeUrl("main-issuer");

            Assert.Equal("https://fcloud3.example.com/f3sso/iss?clientId=registered-appA", url);
        }

        [Fact]
        public void BuildAuthorizeUrl_WithEmptyClientId_ShouldUseAudienceId()
        {
            var audience = CreateAudience(audienceId: "appa", issuerClientId: "");

            var url = audience.BuildAuthorizeUrl("main-issuer");

            Assert.Equal("https://fcloud3.example.com/f3sso/iss?clientId=appa", url);
        }

        [Fact]
        public void BuildAuthorizeUrl_WithUnknownIssuer_ShouldReturnNull()
        {
            var audience = CreateAudience();

            var url = audience.BuildAuthorizeUrl("not-exist");

            Assert.Null(url);
        }
    }
}
