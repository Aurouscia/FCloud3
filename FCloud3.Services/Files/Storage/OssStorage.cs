using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Files.Storage
{
    public class OssStorage : IStorage
    {
        private readonly IOssConfig _config;
        private readonly Lazy<OssClient> _ossClient;

        public OssStorage(IOssConfigProvider ossConfigProvider)
        {
            _config = ossConfigProvider.Get();
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
            throw new NotImplementedException();
        }
        public string FullUrl(string pathName)
        {
            return _config.DomainName + "/" + pathName;
        }
    }

    public interface IOssConfig
    {
        public string? EndPoint { get; }
        public string? BucketName { get; }
        public string? AccessKeyId { get; }
        public string? AccessKeySecret { get; }
        public string? DomainName { get; }
    }
    public interface IOssConfigProvider
    {
        public IOssConfig Get();
    }
}
