using Aliyun.OSS;
using FCloud3.Repos.Files;
using FCloud3.Entities.Files;
using System.Text.RegularExpressions;
using FCloud3.Utils.Utils.FileUtils;

namespace FCloud3.Services.Files
{
    public class OssFileItemService:IFileItemService
    {
        private readonly FileItemRepo _repo;
        private readonly IOssConfig _config;
        private readonly Lazy<OssClient> _ossClient;

        public OssFileItemService(FileItemRepo repo, IOssConfig config)
        {
            _repo = repo;
            _config = config;
            _ossClient = new Lazy<OssClient>(()=>new OssClient(config.EndPoint, config.AccessKeyId, config.AccessKeySecret));
        }

        public int Save(Stream stream,int byteCount, string displayName, string storePath, string? storeName, out string? errmsg)
        {
            storeName ??= FileNameUtils.RandRenameFor(displayName);
            var storePathName = StorePathName(storePath, storeName,out errmsg);
            if(storePathName is null) { return 0; }
            FileItem f = new()
            {
                DisplayName = displayName,
                StorePathName = storePathName,
                ByteCount = byteCount
            };
            if (!_repo.TryAddCheck(f, out errmsg)) { return 0; }
            PutObjectResult res;
            try
            {
                res = _ossClient.Value.PutObject(_config.BucketName, storePathName, stream);
            }
            catch (Exception ex)
            {
                errmsg = $"上传文件失败：{ex.Message}";
                return 0;
            }
            stream.Close();
            f.Hash = res.ETag;

            return _repo.TryAddAndGetId(f, out errmsg);
        }
        public static string? StorePathName(string storePath, string storeName, out string? errmsg)
        {
            string pathName = storePath + "/" + storeName;
            if (pathName.Length > FileItem.storePathNameMaxLength)
            {
                errmsg = "存储名过长，请缩短";
                return null;
            }
            if (storeName.StartsWith("/") || storeName.StartsWith("\\"))
            {
                errmsg = "存储名不能以/或\\开头";
                return null;
            }
            if (Regex.IsMatch(storeName, @"//|\.\.|\\\\"))
            {
                errmsg = @"存储名不能有连续的//或..或\\";
                return null;
            }
            errmsg = null;
            return pathName;
        }

        public bool Delete(int id, out string? errmsg)
        {
            throw new NotImplementedException();
        }

        public bool ExistDisplayName(string displayName)
        {
            throw new NotImplementedException();
        }

        public bool ExistStorePathName(string storePathName)
        {
            throw new NotImplementedException();
        }

        public string Url(int id)
        {
            throw new NotImplementedException();
        }

        public string Url(string displayName)
        {
            throw new NotImplementedException();
        }
    }
    public interface IOssConfig
    {
        public string EndPoint { get; }
        public string BucketName { get; }
        public string AccessKeyId { get; }
        public string AccessKeySecret { get; }
    }
}
