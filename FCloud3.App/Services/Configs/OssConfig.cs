using FCloud3.Services.Files;
using FCloud3.Utils.Settings;

namespace FCloud3.App.Services.Configs
{
    public class OssConfig : IOssConfig
    {
        public string EndPoint => AppSettings.Oss.EndPoint ?? throw new Exception("未找到OssEndPoint配置");
        public string BucketName => AppSettings.Oss.BucketName ?? throw new Exception("未找到OssBucketName配置");
        public string AccessKeyId => AppSettings.Oss.AccessKeyId ?? throw new Exception("未找到OssAccessKeyId配置");
        public string AccessKeySecret => AppSettings.Oss.AccessKeySecret ?? throw new Exception("未找到OssAccessKeySecret配置");
        public string DomainName => AppSettings.Oss.DomainName ?? throw new Exception("未找到OssDomainName配置");
    }
}
