using Aliyun.OSS;
using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Services.Files.Storage
{
    public class OssStorage : IStorage
    {
        private readonly OssConfig _config;
        private readonly Lazy<OssClient> _ossClient;
        public StorageProvideType ProvideType => StorageProvideType.Redirect;
        public OssStorage(IConfiguration config)
        {
            _config = new();
            config.GetSection("FileStorage:Oss").Bind(_config);
            _config.SelfCheck();

            _ossClient = new Lazy<OssClient>(() => new OssClient(_config.EndPoint, _config.AccessKeyId, _config.AccessKeySecret));
        }
        public bool Save(Stream s, string pathName, out string? errmsg)
        {
            try
            {
                _ossClient.Value.PutObject(_config.BucketName, pathName, s);
            }
            catch(Exception ex)
            {
                errmsg = ex.Message;
                return false;
            }
            errmsg = null;
            return true;
        }
        public bool Delete(string pathName, out string? errmsg)
        {
            try
            {
                _ossClient.Value.DeleteObject(_config.BucketName, pathName);
            }
            catch(Exception ex)
            {
                errmsg = ex.Message;
                return false;
            }
            errmsg = null;
            return true;
        }
        public string FullUrl(string pathName)
        {
            return _config.DomainName + "/" + pathName;
        }
        public Stream? Read(string pathName)
        {
            var obj = _ossClient.Value.GetObject(_config.BucketName, pathName);
            return obj.ResponseStream;
        }
    }

    internal class OssConfig
    {
        public string? EndPoint { get; set; }
        public string? BucketName { get; set; }
        public string? AccessKeyId { get; set; }
        public string? AccessKeySecret { get; set; }
        public string? DomainName { get; set; }
        public void SelfCheck()
        {
            if (string.IsNullOrWhiteSpace(EndPoint))
                throw new Exception("未找到配置项FileStorage:Oss:EndPoint");
            if (string.IsNullOrWhiteSpace(BucketName))
                throw new Exception("未找到配置项FileStorage:Oss:BucketName");
            if (string.IsNullOrWhiteSpace(AccessKeyId))
                throw new Exception("未找到配置项FileStorage:Oss:AccessKeyId");
            if (string.IsNullOrWhiteSpace(AccessKeySecret))
                throw new Exception("未找到配置项FileStorage:Oss:AccessKeySecret");
            if (string.IsNullOrWhiteSpace(DomainName))
                throw new Exception("未找到配置项FileStorage:Oss:DomainName");
        }
    }
}
