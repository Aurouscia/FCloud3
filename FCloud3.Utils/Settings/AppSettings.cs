namespace FCloud3.Utils.Settings;

public class AppSettings
{
    public class Jwt
    {
        public static string? Name { get; set; } = SettingsHelper.Get("Jwt", "Name");
        public static string? Domain { get; set; } = SettingsHelper.Get("Jwt", "Domain");
        public static string? SecretKey { get; set; } = SettingsHelper.Get("Jwt", "SecretKey");
    }
    public class Db
    {
        public static string? Type { get; set; } = SettingsHelper.Get("Db", "Type");
        public static string? ConnStr { get; set; } = SettingsHelper.Get("Db", "ConnStr");
    }
    public class Oss
    {
        public static string? EndPoint { get; set; } = SettingsHelper.Get("Oss", "EndPoint");
        public static string? BucketName { get; set; } = SettingsHelper.Get("Oss", "BucketName");
        public static string? AccessKeyId { get; set; } = SettingsHelper.Get("Oss", "AccessKeyId");
        public static string? AccessKeySecret { get; set; } = SettingsHelper.Get("Oss", "AccessKeySecret");
        public static string? DomainName { get; set; } = SettingsHelper.Get("Oss", "DomainName");
    }
}