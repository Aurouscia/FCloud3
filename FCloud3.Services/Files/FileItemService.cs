using FCloud3.Repos.Files;
using FCloud3.Entities.Files;
using FCloud3.Services.Files.Storage.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using FCloud3.Entities.Messages;
using FCloud3.Repos.Messages;

namespace FCloud3.Services.Files
{
    public class FileItemService
    {
        private readonly FileItemRepo _fileItemRepo;
        private readonly FileDirRepo _fileDirRepo;
        private readonly IStorage _storage;
        private readonly OpRecordRepo _opRecordRepo; 

        public FileItemService(FileItemRepo fileItemRepo, FileDirRepo fileDirRepo, IStorage storage, OpRecordRepo opRecordRepo)
        {
            _fileItemRepo = fileItemRepo;
            _fileDirRepo = fileDirRepo;
            _storage = storage;
            _opRecordRepo = opRecordRepo;
        }

        public FileItemDetail? GetDetail(int id, out string? errmsg)
        {
            var item = _fileItemRepo.GetById(id);
            if (item is null)
            {
                errmsg = "找不到指定id的文件";
                return null;
            }
            var path = _fileDirRepo.GetPathById(item.InDir);
            if (path is null)
            {
                errmsg = "寻找指定文件的文件夹路径时出错";
                return null;
            }
            FileItemDetail d = new()
            {
                ItemInfo = item,
                DirPath = path,
            };
            errmsg = null;
            return d;
        }
        public bool EditInfo(int id, string name, out string? errmsg)
        {
            var item = _fileItemRepo.GetById(id);
            if (item is null)
            {
                errmsg = "找不到指定id的文件";
                return false;
            }
            item.DisplayName = name;
            return _fileItemRepo.TryEdit(item, out errmsg);
        }
        public bool Delete(int id, out string? errmsg)
        {
            var item = _fileItemRepo.GetById(id);
            if (item is null)
            {
                errmsg = "找不到指定id的文件";
                return false;
            }
            if (item.StorePathName != null)
            {
                if(!_storage.Delete(item.StorePathName, out errmsg))
                    return false;
            }
            return _fileItemRepo.TryRemove(item, out errmsg);
        }
        public int Save(Stream stream, int byteCount, string displayName, string storePath, string? storeName, string? hash, out string? errmsg)
        {
            if (storeName is null)
            {
                string ext = Path.GetExtension(displayName);
                if (string.IsNullOrEmpty(ext))
                {
                    errmsg = "请勿上传没有扩展名的文件";
                    return 0;
                }
                string randName = Path.GetRandomFileName();
                storeName = Path.ChangeExtension(randName, ext);
            }
            var storePathName = StorePathName(storePath, storeName, out errmsg);
            if (storePathName is null || errmsg is not null)
                return 0;

            FileItem f = new()
            {
                DisplayName = displayName,
                StorePathName = storePathName,
                ByteCount = byteCount,
                Hash = hash,
            };
            if (!_fileItemRepo.TryAddCheck(f, out errmsg)) { return 0; }

            if (ShouldCompress(storeName, byteCount))
            {
                using MemoryStream compressResult = new();
                if (!Compress(stream, compressResult, 1024, out errmsg))
                    return 0;
                compressResult.Seek(0, SeekOrigin.Begin);
                f.ByteCount = (int)compressResult.Length;
                if (!_storage.Save(compressResult, storePathName, out errmsg))
                    return 0;
            }
            else
            {
                if (!_storage.Save(stream, storePathName, out errmsg))
                    return 0;
            }
            var createdId = _fileItemRepo.TryAddAndGetId(f, out errmsg);
            if(createdId > 0)
            {
                _opRecordRepo.Record(OpRecordOpType.Create, OpRecordTargetType.FileItem, $"{displayName}");
                return createdId;
            }
            return 0;
        }

        private static readonly string[] invalidChars = ["/", "\\", ":", "*", "?", ":", "<", ">", "|"];
        public string? StorePathName(string storePath, string storeName, out string? errmsg)
        {
            string pathName = storePath + "/" + storeName;
            if (pathName.Length > FileItem.storePathNameMaxLength)
            {
                errmsg = "存储名过长，请缩短";
                return null;
            }
            if (invalidChars.Any(storeName.Contains))
            {
                errmsg = $"存储名不能含有{string.Join('或', invalidChars)}";
                return null;
            }
            errmsg = null;
            return pathName;
        }
        public string? Url(int id)
        {
            string? storePathName = _fileItemRepo.GetStorePathName(id);
            if (storePathName is null)
                return null;
            return _storage.FullUrl(storePathName);
        }

        //仅压缩jpg格式图片，压缩png有爆内存风险
        private readonly List<string> needCompressExts = [".jpg", ".jpeg"];
        private const int compressLowerThrs = 512 * 1024;
        private const int compressUpperThrs = 5 * 1024 * 1024;
        public bool ShouldCompress(string fileName, int byteCount)
        {
            if (byteCount > compressLowerThrs && byteCount < compressUpperThrs)
                if (needCompressExts.Any(Path.GetExtension(fileName).ToLower().EndsWith))
                    return true;
            return false;
        }
        private static bool Compress(Stream s, Stream output, int maxSide, out string? errmsg)
        {
            Image img;
            try
            {
                img = Image.Load(s);
            }
            catch
            {
                errmsg = "图片格式异常";
                return false;
            }
            if (img.Size.Width <= maxSide && img.Size.Height <= maxSide)
            {
                img.SaveAsJpeg(output);
                img.Dispose();
                errmsg = null;
                return true;
            }
            int w = img.Width;
            int h = img.Height;
            float ratio = (float)w / h;
            Size newSize;
            if (w > h)
                newSize = new Size(maxSide, (int)(maxSide / ratio));
            else if (w < h)
                newSize = new Size((int)(maxSide * ratio), maxSide);
            else
                newSize = new Size(maxSide, maxSide);
            img.Mutate(x => x.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center,
                Size = newSize
            }));
            img.SaveAsJpeg(output);
            img.Dispose();
            errmsg = null;
            return true;
        }
        
        public FileItemReadResult? Read(int id, out string? errmsg)
        {
            var target = _fileItemRepo.GetById(id);
            if (target is null || target.StorePathName is null)
            {
                errmsg = "找不到指定文件";
                return null;
            }
            var pathName = target.StorePathName;
            var stream = _storage.Read(pathName);
            if(stream is null)
            {
                errmsg = "文件丢失";
                return null;
            }
            errmsg = null;
            return new(stream, target.DisplayName ?? "??");
        }
        public class FileItemReadResult(Stream s, string downloadName)
        {
            public Stream Stream { get; set; } = s;
            public string DownloadName { get; set; } = downloadName;
        }

        public class FileItemDetail
        {
            public FileItem? ItemInfo { get; set; }
            public string[]? DirPath { get; set; }
        }
    }
}
