using Aliyun.OSS;
using FCloud3.Repos.Files;
using FCloud3.Entities.Files;
using System.Text.RegularExpressions;
using FCloud3.Utils.Utils.FileUtils;
using FCloud3.Services.Files.Storage;

namespace FCloud3.Services.Files
{
    public class FileItemService
    {
        private readonly FileItemRepo _repo;
        private readonly IStorage _storage;
        private readonly IFileStreamHasher _fileStreamHasher;

        public FileItemService(FileItemRepo repo, IStorage storage, IFileStreamHasher fileStreamHasher)
        {
            _repo = repo;
            _storage = storage;
            _fileStreamHasher = fileStreamHasher;
        }

        public int Save(Stream stream,int byteCount, string displayName, string storePath, string? storeName, out string? errmsg)
        {
            storeName ??= FileNameUtils.RandRenameFor(displayName);
            var storePathName = StorePathName(storePath, storeName,out errmsg);
            if(storePathName is null) { return 0; }
            string hash = _fileStreamHasher.Hash(stream, out stream);
            FileItem f = new()
            {
                DisplayName = displayName,
                StorePathName = storePathName,
                ByteCount = byteCount,
                Hash = hash
            };
            if (!_repo.TryAddCheck(f, out errmsg)) { return 0; }

            if (!_storage.Save(stream, storePathName, out errmsg))
                return 0;
            stream.Close();
            return _repo.TryAddAndGetId(f, out errmsg);
        }
        public string? StorePathName(string storePath, string storeName, out string? errmsg)
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

        public string? Url(int id)
        {
            string? storePathName = _repo.GetStorePathName(id);
            if (storePathName is null)
                return null;
            return _storage.FullUrl(storePathName);
        }
    }
}
