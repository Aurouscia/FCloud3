using FCloud3.Services.Files.Storage;

namespace FCloud3.App.Services.Configs
{
    public class OssConfig : IOssConfig
    {
        public string? EndPoint { get; set; }
        public string? BucketName { get; set; }
        public string? AccessKeyId { get; set; }
        public string? AccessKeySecret { get; set; }
        public string? DomainName { get; set; }
    }
    public class OssConfigProvider : IOssConfigProvider
    {
        private readonly OssConfig _config;
        private const string configPath = "FileStorage:Oss";
        public OssConfigProvider(IConfiguration appConfig)
        {
            var section = appConfig.GetSection(configPath);
            _config = new OssConfig();
            section.Bind(_config);
            ConfigCheck.Check(_config, configPath);
        }

        public IOssConfig Get()
        {
            return _config;
        }
    }
}
