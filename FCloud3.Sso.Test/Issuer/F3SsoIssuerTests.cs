using FCloud3.Sso.Issuer;
using FCloud3.Sso.Test.Support;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace FCloud3.Sso.Test.Issuer
{
    public class F3SsoIssuerTests
    {
        private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }

        private static F3SsoIssuerService CreateIssuerService(
            IMemoryCache cache,
            IConfiguration configuration,
            int userId = 1,
            string userName = "tester",
            byte userLevel = 1)
        {
            var userInfo = new FakeUserInfoProvider(userId, userName, userLevel);
            return new F3SsoIssuerService(cache, configuration, userInfo, NullLogger<F3SsoIssuerService>.Instance);

        }

        [Fact]
        public void StoreCode_ShouldReturnNonEmptyCode_AndRetrieveSameUser()
        {
            var values = new Dictionary<string, string?>
            {
                ["F3Sso:Enabled"] = "true",
                ["F3Sso:Audiences:0:Id"] = "appA",
                ["F3Sso:Audiences:0:RequireLevel"] = "1"
            };
            var cache = new MemoryCache(new MemoryCacheOptions());
            var config = BuildConfiguration(values);
            var service = CreateIssuerService(cache, config, userId: 42, userName: "Alice", userLevel: 2);

            var code = service.StoreCode();

            Assert.False(string.IsNullOrEmpty(code));
            Assert.True(service.TryGetUser(code, out var user));
            Assert.NotNull(user);
            Assert.Equal(42, user!.Id);
            Assert.Equal("Alice", user.Name);
            Assert.Equal((byte)2, user.Type);
        }

        [Fact]
        public void CurrentUserMeetsLevel_WhenDisabled_ShouldReturnFalse()
        {
            var values = new Dictionary<string, string?>
            {
                ["F3Sso:Enabled"] = "false",
                ["F3Sso:Audiences:0:Id"] = "appA",
                ["F3Sso:Audiences:0:RequireLevel"] = "0"
            };
            var cache = new MemoryCache(new MemoryCacheOptions());
            var config = BuildConfiguration(values);
            var service = CreateIssuerService(cache, config, userLevel: 9);

            Assert.False(service.CurrentUserMeetsLevel("appA"));
        }

        [Fact]
        public void CurrentUserMeetsLevel_WhenAudienceNotFound_ShouldReturnFalse()
        {
            var values = new Dictionary<string, string?>
            {
                ["F3Sso:Enabled"] = "true",
                ["F3Sso:Audiences:0:Id"] = "appA",
                ["F3Sso:Audiences:0:RequireLevel"] = "1"
            };
            var cache = new MemoryCache(new MemoryCacheOptions());
            var config = BuildConfiguration(values);
            var service = CreateIssuerService(cache, config, userLevel: 9);

            Assert.False(service.CurrentUserMeetsLevel("appB"));
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(2, 1, true)]
        [InlineData(8, 1, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 9, false)]
        public void CurrentUserMeetsLevel_ShouldCompareUserLevel(int userLevel, int requiredLevel, bool expected)
        {
            var values = new Dictionary<string, string?>
            {
                ["F3Sso:Enabled"] = "true",
                ["F3Sso:Audiences:0:Id"] = "appA",
                ["F3Sso:Audiences:0:RequireLevel"] = requiredLevel.ToString()
            };
            var cache = new MemoryCache(new MemoryCacheOptions());
            var config = BuildConfiguration(values);
            var service = CreateIssuerService(cache, config, userLevel: (byte)userLevel);

            Assert.Equal(expected, service.CurrentUserMeetsLevel("appA"));
        }
    }
}
