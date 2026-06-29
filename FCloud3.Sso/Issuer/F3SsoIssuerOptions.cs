namespace FCloud3.Sso.Issuer
{
    /// <summary>
    /// SSO 签发方（Issuer）配置，定义在签发站点的 appsettings.json 中。
    /// </summary>
    public class F3SsoIssuerOptions
    {
        /// <summary>
        /// 当前签发方标识，受众方配置中 Issuer 的 Id 应与此一致。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用 SSO 服务。
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 允许接入的受众（Audience）应用列表。
        /// </summary>
        public List<F3SsoIssuerAudienceOptions> Audiences { get; set; } = [];
    }

    /// <summary>
    /// 签发方视角下的一个接入受众配置。
    /// </summary>
    public class F3SsoIssuerAudienceOptions
    {
        /// <summary>
        /// 受众标识，对应授权请求中的 clientId。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 受众显示名称，用于在授权页面向用户展示。
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 受众站点基础地址，用于构造回调跳转 URL。
        /// </summary>
        public string Origin { get; set; } = string.Empty;

        /// <summary>
        /// 受众头像 URL，用于在授权页面展示。
        /// </summary>
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// 要求的最低用户等级，对应 <see cref="FCloud3.Entities.Identities.UserType"/> 的整数值。
        /// </summary>
        public int RequireLevel { get; set; }
    }
}
