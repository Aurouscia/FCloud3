using System.Net;
using System.Net.Http.Json;
using FCloud3.Sso.Audience;
using FCloud3.Sso.Issuer;
using FCloud3.Sso.Test.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FCloud3.Sso.Test.Audience
{
    public class F3SsoAudienceEndpointTests
    {
        [Fact]
        public async Task ConfigEndpoint_ShouldReturnAudienceOptions()
        {
            await using var fixture = new AudienceEndpointFixture();
            var client = fixture.AudienceServer.CreateClient();

            var response = await client.GetAsync("/f3sso/aud/config");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var options = await response.Content.ReadFromJsonAsync<F3SsoAudienceOptions>();
            Assert.NotNull(options);
            Assert.Equal("/sso/callback", options!.RedirectPath);
            Assert.Single(options.Issuers);
            Assert.Equal("issuer-test", options.Issuers[0].Id);
        }

        [Fact]
        public async Task AuthEndpoint_WithValidIssuer_ShouldRedirectToIssuerEntry()
        {
            await using var fixture = new AudienceEndpointFixture();
            var client = new HttpClient(fixture.AudienceServer.CreateHandler())
            {
                BaseAddress = fixture.AudienceServer.BaseAddress
            };

            var response = await client.GetAsync("/f3sso/aud/auth?issuerId=issuer-test");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            Assert.Contains("/f3sso/iss/entry?clientId=appA", location);
        }

        [Fact]
        public async Task AuthEndpoint_WithUnknownIssuer_ShouldReturnBadRequest()
        {
            await using var fixture = new AudienceEndpointFixture();
            var client = fixture.AudienceServer.CreateClient();

            var response = await client.GetAsync("/f3sso/aud/auth?issuerId=not-exist");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidateEndpoint_WithValidCode_ShouldRedirectWithSuccess()
        {
            await using var fixture = new AudienceEndpointFixture();
            var issuerClient = fixture.IssuerServer.CreateClient();
            var audienceClient = fixture.AudienceServer.CreateClient();

            var issueResponse = await issuerClient.GetAsync("/f3sso/iss?clientId=appA");
            var codeJson = await issueResponse.Content.ReadFromJsonAsync<CodeResponse>();
            var code = codeJson!.Code;

            var response = await audienceClient.GetAsync($"/f3sso/aud/validate?code={code}&issuerId=issuer-test");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            Assert.StartsWith("/sso/callback?success=1", location);
        }

        [Fact]
        public async Task ValidateEndpoint_WithValidCode_ShouldInvokeSignInHandler()
        {
            var handler = new FakeSignInHandler();
            await using var fixture = new AudienceEndpointFixture(handler);
            var issuerClient = fixture.IssuerServer.CreateClient();
            var audienceClient = fixture.AudienceServer.CreateClient();

            var issueResponse = await issuerClient.GetAsync("/f3sso/iss?clientId=appA");
            var codeJson = await issueResponse.Content.ReadFromJsonAsync<CodeResponse>();
            var code = codeJson!.Code;

            await audienceClient.GetAsync($"/f3sso/aud/validate?code={code}&issuerId=issuer-test");

            Assert.True(handler.Called);
            Assert.Equal("issuer-test", handler.ReceivedIssuerId);
            Assert.NotNull(handler.ReceivedUser);
            Assert.Equal(42, handler.ReceivedUser!.Id);
            Assert.Equal("Alice", handler.ReceivedUser.Name);
        }

        [Fact]
        public async Task ValidateEndpoint_WithInvalidCode_ShouldNotInvokeSignInHandler()
        {
            var handler = new FakeSignInHandler();
            await using var fixture = new AudienceEndpointFixture(handler);
            var client = fixture.AudienceServer.CreateClient();

            await client.GetAsync("/f3sso/aud/validate?code=bad-code&issuerId=issuer-test");

            Assert.False(handler.Called);
        }

        [Fact]
        public async Task ValidateEndpoint_WithInvalidCode_ShouldRedirectWithFailure()
        {
            await using var fixture = new AudienceEndpointFixture();
            var client = fixture.AudienceServer.CreateClient();

            var response = await client.GetAsync("/f3sso/aud/validate?code=bad-code&issuerId=issuer-test");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            Assert.StartsWith("/sso/callback?success=0", location);
            Assert.Contains("errmsg=", location);
        }

        [Fact]
        public async Task ValidateEndpoint_WhenMissingParameters_ShouldRedirectWithFailure()
        {
            await using var fixture = new AudienceEndpointFixture();
            var client = fixture.AudienceServer.CreateClient();

            var response = await client.GetAsync("/f3sso/aud/validate");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            Assert.StartsWith("/sso/callback?success=0", location);
            Assert.Contains("errmsg=", location);
        }

        private sealed class AudienceEndpointFixture : IAsyncDisposable
        {
            public TestServer IssuerServer { get; }
            public TestServer AudienceServer { get; }
            private readonly IHost _issuerHost;
            private readonly IHost _audienceHost;

            public AudienceEndpointFixture(IF3SsoSignInHandler? signInHandler = null)
            {
                var issuerSettings = new Dictionary<string, string?>
                {
                    ["F3Sso:Id"] = "issuer-test",
                    ["F3Sso:Enabled"] = "true",
                    ["F3Sso:Audiences:0:Id"] = "appA",
                    ["F3Sso:Audiences:0:DisplayName"] = "应用A",
                    ["F3Sso:Audiences:0:Origin"] = "https://appa.example.com",
                    ["F3Sso:Audiences:0:Avatar"] = "",
                    ["F3Sso:Audiences:0:RequireLevel"] = "1"
                };

                _issuerHost = new HostBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(issuerSettings));
                        webBuilder.ConfigureServices(services =>
                        {
                            services.AddMemoryCache();
                            services.AddSingleton<IUserInfoProvider>(new FakeUserInfoProvider(42, "Alice", 1));
                            services.AddScoped<F3SsoIssuerService>();
                        });
                        webBuilder.Configure(app =>
                        {
                            app.UseRouting();
                            app.UseEndpoints(endpoints => endpoints.MapF3SsoIssuerEndpoints());
                        });
                        webBuilder.UseTestServer();
                    })
                    .Build();
                _issuerHost.Start();
                IssuerServer = _issuerHost.GetTestServer();

                var audienceSettings = new Dictionary<string, string?>
                {
                    ["F3Sso:RedirectPath"] = "/sso/callback",
                    ["F3Sso:Issuers:0:Id"] = "issuer-test",
                    ["F3Sso:Issuers:0:ClientId"] = "appA",
                    ["F3Sso:Issuers:0:Origin"] = IssuerServer.BaseAddress!.ToString().TrimEnd('/')
                };

                _audienceHost = new HostBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(audienceSettings));
                        webBuilder.ConfigureServices(services =>
                        {
                            services.AddF3SsoAudience();
                            services.AddScoped(sp => new F3SsoAudience(
                                sp.GetRequiredService<IConfiguration>(),
                                IssuerServer.CreateHandler()));
                            if (signInHandler is not null)
                            {
                                services.AddSingleton<IF3SsoSignInHandler>(signInHandler);
                            }
                        });
                        webBuilder.Configure(app =>
                        {
                            app.UseRouting();
                            app.UseEndpoints(endpoints => endpoints.MapF3SsoAudienceEndpoints());
                        });
                        webBuilder.UseTestServer();
                    })
                    .Build();
                _audienceHost.Start();
                AudienceServer = _audienceHost.GetTestServer();
            }

            public async ValueTask DisposeAsync()
            {
                await ((IAsyncDisposable)_audienceHost).DisposeAsync();
                await ((IAsyncDisposable)_issuerHost).DisposeAsync();
            }
        }

        private sealed record CodeResponse(string Code);

        private sealed class FakeSignInHandler : IF3SsoSignInHandler
        {
            public bool Called { get; private set; }
            public F3SsoValidatedUser? ReceivedUser { get; private set; }

            public string? ReceivedIssuerId { get; private set; }

            public Task HandleAsync(Microsoft.AspNetCore.Http.HttpContext context, string issuerId, F3SsoValidatedUser user)
            {
                Called = true;
                ReceivedIssuerId = issuerId;
                ReceivedUser = user;
                return Task.CompletedTask;
            }
        }
    }
}
