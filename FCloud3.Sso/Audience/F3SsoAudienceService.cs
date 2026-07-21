using FCloud3.Sso;
using FCloud3.Sso.Issuer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FCloud3.Sso.Audience
{
    /// <summary>
    /// SSO 受众方客户端，用于向指定签发方请求授权以及验证 code。
    /// </summary>
    public class F3SsoAudienceService
    {
        private readonly F3SsoAudienceOptions _options;
        private readonly HttpMessageHandler? _testHandler;
        private readonly ILogger<F3SsoAudienceService> _logger;

        public F3SsoAudienceService(
            IConfiguration configuration,
            ILogger<F3SsoAudienceService> logger,
            HttpMessageHandler? testHandler = null)
        {
            _options = new F3SsoAudienceOptions();
            configuration.GetSection("F3Sso").Bind(_options);
            _testHandler = testHandler;
            _logger = logger;
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
            return $"{origin}/f3sso/iss/entry?clientId={Uri.EscapeDataString(clientId)}";
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
            using var client = _testHandler is not null
                ? new HttpClient(_testHandler, disposeHandler: false)
                : new HttpClient();
            client.BaseAddress = baseUrl;
            client.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                var response = await client.GetAsync(
                    $"/f3sso/iss/validate/{Uri.EscapeDataString(code)}",
                    cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<F3SsoValidatedUser>(
                        cancellationToken: cancellationToken);
                }
                _logger.LogWarning("签发方 {IssuerId} 验证 code 返回非成功状态码 {StatusCode}", issuerId, response.StatusCode);
                return null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "向签发方 {IssuerId} 验证 code 时发生异常", issuerId);
                return null;
            }
        }
    }
}
