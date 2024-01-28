using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Services.Files.Storage
{
    public class LocalStorage : IStorage
    {
        private readonly string _domainName;
        private readonly string _storeRootPath;
        public StorageProvideType ProvideType => StorageProvideType.Stream;

        public LocalStorage(IConfiguration config)
        {
            var section = config.GetSection("FileStorage:Local")??throw new Exception("未找到配置节FileStorage:Local");
            _storeRootPath = section["Path"] ?? throw new Exception("未找到配置FileStorage:Local:Path");
            _domainName = section["DomainName"] ?? throw new Exception("未找到配置FileStorage:Local:DomainName");
            DirectoryInfo di = new(_storeRootPath);
            if(!di.Exists) { di.Create(); }
        }
        public bool Save(Stream s, string pathName, out string? errmsg)
        {
            try
            {
                string path = Path.Combine(_storeRootPath, pathName);
                string dirPath = Path.GetDirectoryName(path) ?? throw new Exception("保存文件：路径异常");
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                if(!dirInfo.Exists) { dirInfo.Create(); }
                var fs = File.Create(path);
                s.CopyTo(fs);
                s.Flush();
                s.Close();
                fs.Close();
            }
            catch (Exception ex)
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
            return _domainName + "/" + pathName;
        }
        public Stream? Read(string pathName)
        {
            try
            {
                var fs = File.OpenRead(Path.Combine(_storeRootPath, pathName));
                return fs;
            }
            catch
            {
                return null;
            }
        }
    }
}
