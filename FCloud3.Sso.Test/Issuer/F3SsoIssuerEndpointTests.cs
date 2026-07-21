using System.Net;
using System.Net.Http.Json;
using FCloud3.Sso.Issuer;
using FCloud3.Sso.Test.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FCloud3.Sso.Test.Issuer
{
    public class F3SsoIssuerEndpointTests
    {
        private static IssuerEndpointFixture CreateFixture(
            byte userLevel = 1,
            bool enabled = true,
            int audienceRequireLevel = 1,
            string? entryPath = "/sso/entry")
        {
            return new IssuerEndpointFixture(userLevel, enabled, audienceRequireLevel, entryPath);
        }

        [Fact]
        public async Task ConfigEndpoint_ShouldReturnIssuerOptions()
        {
            await using var fixture = CreateFixture();
            var client = fixture.Server.CreateClient();

            var response = await client.GetAsync("/f3sso/iss/config");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var options = await response.Content.ReadFromJsonAsync<F3SsoIssuerOptions>();
            Assert.NotNull(options);
            Assert.Equal("issuer-test", options!.Id);
            Assert.True(options.Enabled);
            Assert.Single(options.Audiences);
            Assert.Equal("appA", options.Audiences[0].Id);
            Assert.Equal("应用A", options.Audiences[0].DisplayName);
        }

        [Fact]
        public async Task IssueEndpoint_WhenLevelMeets_ShouldReturnCode()
        {
            await using var fixture = CreateFixture(userLevel: 1, audienceRequireLevel: 1);
            var client = fixture.Server.CreateClient();

            var response = await client.GetAsync("/f3sso/iss?clientId=appA");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadFromJsonAsync<CodeResponse>();
            Assert.NotNull(json);
            Assert.False(string.IsNullOrEmpty(json!.Code));
        }

        [Fact]
        public async Task IssueEndpoint_WhenLevelNotEnough_ShouldReturnBadRequest()
        {
            await using var fixture = CreateFixture(userLevel: 0, audienceRequireLevel: 1);
            var client = fixture.Server.CreateClient();

            var response = await client.GetAsync("/f3sso/iss?clientId=appA");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("等级", message);
        }

        [Fact]
        public async Task ValidateEndpoint_WithValidCode_ShouldReturnUser()
        {
            await using var fixture = CreateFixture(userLevel: 2);
            var client = fixture.Server.CreateClient();

            var issueResponse = await client.GetAsync("/f3sso/iss?clientId=appA");
            var codeJson = await issueResponse.Content.ReadFromJsonAsync<CodeResponse>();
            var code = codeJson!.Code;

            var validateResponse = await client.GetAsync($"/f3sso/iss/validate/{code}");

            Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);
            var user = await validateResponse.Content.ReadFromJsonAsync<F3SsoValidatedUser>();
            Assert.NotNull(user);
            Assert.Equal(42, user!.Id);
            Assert.Equal("Alice", user.Name);
            Assert.Equal((byte)2, user.Type);
        }

        [Fact]
        public async Task ValidateEndpoint_WithInvalidCode_ShouldReturnBadRequest()
        {
            await using var fixture = CreateFixture();
            var client = fixture.Server.CreateClient();

            var response = await client.GetAsync("/f3sso/iss/validate/not-exist-code");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private sealed class IssuerEndpointFixture : IAsyncDisposable
        {
            public TestServer Server { get; }
            private readonly IHost _host;

            public IssuerEndpointFixture(byte userLevel, bool enabled, int audienceRequireLevel, string? entryPath)
            {
                var settings = new Dictionary<string, string?>
                {
                    ["F3Sso:Id"] = "issuer-test",
                    ["F3Sso:Enabled"] = enabled.ToString(),
                    ["F3Sso:Audiences:0:Id"] = "appA",
                    ["F3Sso:Audiences:0:DisplayName"] = "应用A",
                    ["F3Sso:Audiences:0:Origin"] = "https://appa.example.com",
                    ["F3Sso:Audiences:0:Avatar"] = "",
                    ["F3Sso:Audiences:0:RequireLevel"] = audienceRequireLevel.ToString()
                };
                if (entryPath is not null)
                {
                    settings["F3Sso:EntryPath"] = entryPath;
                }

                _host = new HostBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(settings));
                        webBuilder.ConfigureServices(services =>
                        {
                            services.AddMemoryCache();
                            services.AddSingleton<IUserInfoProvider>(new FakeUserInfoProvider(42, "Alice", userLevel));
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
                _host.Start();
                Server = _host.GetTestServer();
            }

            public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();
        }

        [Fact]
        public async Task EntryEndpoint_WithQuery_ShouldRedirectToEntryPathPreservingQuery()
        {
            await using var fixture = CreateFixture();
            var client = new HttpClient(fixture.Server.CreateHandler()) { BaseAddress = fixture.Server.BaseAddress };

            var response = await client.GetAsync("/f3sso/iss/entry?target=%2Ffoo&source=bar");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/sso/entry?target=%2Ffoo&source=bar", response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task EntryEndpoint_WithoutQuery_ShouldRedirectToEntryPath()
        {
            await using var fixture = CreateFixture();
            var client = new HttpClient(fixture.Server.CreateHandler()) { BaseAddress = fixture.Server.BaseAddress };

            var response = await client.GetAsync("/f3sso/iss/entry");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/sso/entry", response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task EntryEndpoint_WithoutConfiguredEntryPath_ShouldReturnInternalServerError()
        {
            await using var fixture = CreateFixture(entryPath: null);
            var client = fixture.Server.CreateClient();

            var response = await client.GetAsync("/f3sso/iss/entry?x=1");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private sealed record CodeResponse(string Code);
    }
}
