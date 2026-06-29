using Microsoft.Extensions.DependencyInjection;

namespace FCloud3.Sso.Audience
{
    public static class F3SsoAudienceServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 Audience 端点所需的 SSO 服务，包括默认的 <see cref="NoOpF3SsoSignInHandler"/>。
        /// 如需自定义登录行为，请在调用此方法后注册自己的 <see cref="IF3SsoSignInHandler"/> 实现。
        /// </summary>
        public static IServiceCollection AddF3SsoAudience(this IServiceCollection services)
        {
            services.AddScoped<F3SsoAudienceService>();
            services.AddScoped<IF3SsoSignInHandler, NoOpF3SsoSignInHandler>();
            return services;
        }
    }
}
