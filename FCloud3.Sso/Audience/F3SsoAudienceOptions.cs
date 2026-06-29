namespace FCloud3.Sso.Audience
{
    /// <summary>
    /// SSO 受众（Audience）配置，定义在使用方应用的配置文件中。
    /// </summary>
    public class F3SsoAudienceOptions
    {
        /// <summary>
        /// 当前受众方标识。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用 SSO 登录功能。
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 允许用户选择登录的 SSO 签发方（Issuer）列表。
        /// </summary>
        public List<F3SsoAudienceIssuerOptions> Issuers { get; set; } = [];

        /// <summary>
        /// 验证成功后重定向到的路径。
        /// </summary>
        public string RedirectPath { get; set; } = "/";
    }

    /// <summary>
    /// 受众视角下的一个签发方配置。
    /// </summary>
    public class F3SsoAudienceIssuerOptions
    {
        /// <summary>
        /// 签发方标识，用于区分不同的签发源。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 签发方显示名称，用于在登录入口向用户展示。
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 签发方基础地址，用于拼接 /f3sso 相关请求路径。
        /// </summary>
        public string Origin { get; set; } = string.Empty;

        /// <summary>
        /// 签发方头像 URL，用于在登录入口展示。
        /// </summary>
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// 当前应用在对应签发方注册的客户端 Id。
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
    }
}
