using FCloud3.Services.Identities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace FCloud3.App.Controllers.Identities
{
    [Route("connect")]
    public class OidcController : Controller
    {
        private readonly UserService _userService;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly ILogger<OidcController> _logger;

        public OidcController(
            UserService userService,
            IOpenIddictApplicationManager applicationManager,
            ILogger<OidcController> logger)
        {
            _userService = userService;
            _applicationManager = applicationManager;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request is null)
            {
                return BadRequest(new { error = Errors.InvalidRequest, error_description = "无法解析 OIDC 授权请求" });
            }

            // 校验客户端
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
            if (application is null)
            {
                return BadRequest(new { error = Errors.InvalidClient, error_description = "客户端不存在" });
            }

            // 从现有 JWT 中提取用户 ID
            var nameIdClaim = User.Claims.FirstOrDefault(c =>
                c.Type == Claims.Subject ||
                c.Type == JwtRegisteredClaimNames.NameId ||
                c.Type == ClaimTypes.NameIdentifier);

            if (nameIdClaim is null || !int.TryParse(nameIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var user = _userService.GetById(userId);
            if (user is null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("OIDC 授权请求：用户 {UserId} 客户端 {ClientId}", userId, request.ClientId);

            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name,
                Claims.Role);

            identity.SetClaim(Claims.Subject, user.Id.ToString());
            identity.SetClaim(Claims.Name, user.Name);
            identity.SetClaim(Claims.PreferredUsername, user.Name);

            identity.SetScopes(request.GetScopes());
            identity.SetResources("resource_server");

            identity.SetDestinations(static claim => claim.Type switch
            {
                Claims.Name or Claims.PreferredUsername => new[] { Destinations.AccessToken, Destinations.IdentityToken },
                _ => new[] { Destinations.AccessToken, Destinations.IdentityToken }
            });

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            // 返回 OpenIddict 授权结果，由框架生成 code 并重定向
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
