namespace FCloud3.Sso
{
    /// <summary>
    /// SSO 验证成功后返回的用户信息。
    /// </summary>
    public class F3SsoValidatedUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte Type { get; set; }
    }
}
