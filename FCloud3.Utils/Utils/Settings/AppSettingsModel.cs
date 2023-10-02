using FCloud3.Utils.Utils.Logger;

namespace FCloud3.Utils.Utils.Settings
{
    public class AppSettingsModel
    {
        public FLoggerSettings? FLogger { get; set; }
        public SiteInfos? SiteInfo { get; set; }
        public StorageInfos? StorageInfo { get; set; }
        public ConnStrInfos? ConnectionStrings { get; set; }
        public class SiteInfos
        {
            public string? Name { get; set; }
            public string? Domain { get; set; }
            public string? JwtSecretKey { get; set; }
        }
        public class StorageInfos
        {
            public string? StoragePath { get; set; }
            public string? StoragePathEx { get; set; }
        }
        public class ConnStrInfos
        {
            public string? SqlServer { get; set; }
        }
    }
}
