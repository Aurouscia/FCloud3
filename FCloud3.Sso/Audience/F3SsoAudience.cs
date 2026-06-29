using FCloud3.Sso;
using FCloud3.Sso.Issuer;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace FCloud3.Sso.Audience
{
    /// <summary>
    /// SSO 受众方客户端，用于向指定签发方请求授权以及验证 code。
    /// </summary>
    public class F3SsoAudience
    {
        private readonly F3SsoAudienceOptions _options;
        private readonly HttpMessageHandler? _testHandler;

        public F3SsoAudience(IConfiguration configuration, HttpMessageHandler? testHandler = null)
        {
            _options = new F3SsoAudienceOptions();
            configuration.GetSection("F3Sso").Bind(_options);
            _testHandler = testHandler;
        }

        private F3SsoAudienceIssuerOptions? GetIssuer(string issuerId)
        {
            return _options.Issuers.FirstOrDefault(i => i.Id == issuerId);
        }

        /// <summary>
        /// 构造需要跳转到的授权页面 URL。
        /// </summary>
        /// <param name="issuerId">签发方标识</param>
        public string? BuildAuthorizeUrl(string issuerId)
        {
            var issuer = GetIssuer(issuerId);
            if (issuer is null)
                return null;
            var origin = issuer.Origin.TrimEnd('/');
            var clientId = GetClientId(issuer);
            return $"{origin}/f3sso/iss?clientId={Uri.EscapeDataString(clientId)}";
        }

        private string GetClientId(F3SsoAudienceIssuerOptions issuer)
        {
            return string.IsNullOrEmpty(issuer.ClientId) ? _options.Id : issuer.ClientId;
        }

        /// <summary>
        /// 使用 code 向指定签发方验证并换取用户信息。
        /// </summary>
        /// <param name="issuerId">签发方标识</param>
        /// <param name="code">授权码</param>
        public async Task<F3SsoValidatedUser?> ValidateCodeAsync(string issuerId, string code, CancellationToken cancellationToken = default)
        {
            var issuer = GetIssuer(issuerId);
            if (issuer is null)
                return null;

            var baseUrl = new Uri(issuer.Origin.TrimEnd('/'));
            RestClient client;
            if (_testHandler is not null)
            {
                client = new RestClient(_testHandler, disposeHandler: false, configureRestClient: options =>
                {
                    options.BaseUrl = baseUrl;
                    options.ThrowOnAnyError = false;
                    options.Timeout = TimeSpan.FromSeconds(10);
                });
            }
            else
            {
                client = new RestClient(new RestClientOptions
                {
                    BaseUrl = baseUrl,
                    ThrowOnAnyError = false,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }

            var request = new RestRequest($"/f3sso/iss/validate/{Uri.EscapeDataString(code)}");
            var response = await client.ExecuteGetAsync<F3SsoValidatedUser>(request, cancellationToken);
            if (response is not null && response.IsSuccessful)
            {
                return response.Data;
            }
            return null;
        }
    }
}
