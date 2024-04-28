using FCloud3.Entities.Files;
using FCloud3.Repos;
using FCloud3.Repos.Files;
using FCloud3.Services.Etc;
using FCloud3.Services.Etc.Metadata;
using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FCloud3.Services.Files
{
    public class MaterialService
    {
        private readonly MaterialRepo _materialRepo;
        private readonly CacheExpTokenService _cacheExpTokenService;
        private readonly IOperatingUserIdProvider _userIdProvider;
        private readonly IStorage _storage;
        private readonly MaterialMetadataService _materialMetadataService;
        private readonly static object materialNamingLock = new();

        public MaterialService(
            MaterialRepo materialRepo,
            CacheExpTokenService cacheExpTokenService,
            IOperatingUserIdProvider userIdProvider,
            IStorage storage,
            MaterialMetadataService materialMetadataService)
        {
            _materialRepo = materialRepo;
            _cacheExpTokenService = cacheExpTokenService;
            _userIdProvider = userIdProvider;
            _storage = storage;
            _materialMetadataService = materialMetadataService;
        }

        public IndexResult<MaterialIndexItem> Index(IndexQuery q, bool onlyMine)
        {
            IQueryable<Material> from;
            if (onlyMine)
            {
                var userId = _userIdProvider.Get();
                from = _materialRepo.Existing.Where(x => x.CreatorUserId == userId);
            }
            else
                from = _materialRepo.Existing;
            var m = _materialRepo.IndexFilterOrder(from, q).TakePage(q, x => new MaterialIndexItem(x, _storage.FullUrl));
            return m;
        }

        public int Add(Stream stream, string formFileName, string path, string? name, string? desc, out string? errmsg)
        {
            lock (materialNamingLock)
            {
                using MemoryStream compressed = new();
                if (string.IsNullOrWhiteSpace(name))
                {
                    errmsg = "素材名称不能为空";
                    return 0;
                }
                if (name.Length < 2)
                {
                    errmsg = "素材名称最少两个字符";
                    return 0;
                }
                if (name.Length > Material.displayNameMaxLength)
                    name = name[..Material.displayNameMaxLength];
                if (desc is not null && desc.Length > Material.descMaxLength)
                    desc = desc[..Material.descMaxLength];
                if (_materialRepo.Existing.Any(x => x.Name == name))
                {
                    errmsg = "已存在同名素材";
                    return 0;
                }

                string ext;
                if (CanCompress(formFileName))
                {
                    if (!CompressImage(stream, compressed, out errmsg))
                        return 0;
                    ext = ".png";
                }
                else if(CanSaveButNoCompress(formFileName))
                {
                    stream.CopyTo(compressed);
                    if(compressed.Length > noCompressMaxSize)
                    {
                        errmsg = noCompressMaxSizeExceedMsg;
                        return 0;
                    }
                    ext = Path.GetExtension(formFileName);
                }
                else
                {
                    errmsg = "不支持的文件格式";
                    return 0;
                }
                stream.Close();
                compressed.Seek(0, SeekOrigin.Begin);

                string storeName = Path.ChangeExtension(Path.GetRandomFileName(), ext);
                string storePathName = StorePathName(path, storeName);
                if (!_storage.Save(compressed, storePathName, out errmsg))
                    return 0;
                Material m = new()
                {
                    Name = name,
                    StorePathName = storePathName,
                    Desc = desc
                };
                var createdId = _materialRepo.TryAddAndGetId(m, out errmsg);
                if (errmsg is null)
                {
                    _cacheExpTokenService.MaterialNamePathInfo.CancelAll();
                    _materialMetadataService.Create(createdId, name, storePathName);
                }
                return createdId;
            }
        }

        public bool UpdateContent(int id, Stream stream, string formFileName, string path, out string? errmsg)
        {
            Material? m = _materialRepo.GetById(id);
            if (m is null)
            {
                errmsg = "找不到指定素材";
                return false;
            }

            using MemoryStream compressed = new();

            string ext;
            if (CanCompress(formFileName))
            {
                if (!CompressImage(stream, compressed, out errmsg))
                    return false;
                ext = ".png";
            }
            else if (CanSaveButNoCompress(formFileName))
            {
                stream.CopyTo(compressed);
                if(compressed.Length > noCompressMaxSize)
                {
                    errmsg = noCompressMaxSizeExceedMsg;
                    return false;
                }
                ext = Path.GetExtension(formFileName);
            }
            else
            {
                errmsg = "不支持的文件格式";
                return false;
            }
            compressed.Seek(0, SeekOrigin.Begin);

            string storeName = Path.ChangeExtension(Path.GetRandomFileName(), ext);
            string storePathName = StorePathName(path, storeName);
            if (!_storage.Save(compressed, storePathName, out errmsg))
                return false;
            string? oldPathName = m.StorePathName;
            if (oldPathName != null)
                _storage.Delete(oldPathName, out _);
            m.StorePathName = storePathName;
            var success = _materialRepo.TryEdit(m, out errmsg);
            if (success)
            {
                _cacheExpTokenService.MaterialNamePathInfo.CancelAll();
                _materialMetadataService.Update(id, md => md.PathName = storePathName);
            }
            return success;
        }

        public bool UpdateInfo(int id, string name, string? desc, out string? errmsg)
        {
            lock (materialNamingLock)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    errmsg = "素材名称不能为空";
                    return false;
                }
                if (name.Length < 2)
                {
                    errmsg = "素材名称最少两个字符";
                    return false;
                }
                var m = _materialRepo.GetById(id);
                if (m is null)
                {
                    errmsg = "找不到指定素材";
                    return false;
                }
                if (name.Length > Material.displayNameMaxLength)
                    name = name[..Material.displayNameMaxLength];
                if (desc is not null && desc.Length > Material.descMaxLength)
                    desc = desc[..Material.descMaxLength];
                if (_materialRepo.ExistingExceptId(id).Any(x => x.Name == name))
                {
                    errmsg = "已存在同名素材";
                    return false;
                }
                bool nameChanged = m.Name != name;
                m.Name = name;
                m.Desc = desc;
                var success = _materialRepo.TryEdit(m, out errmsg);
                if(success && nameChanged)
                {
                    _cacheExpTokenService.MaterialNamePathInfo.CancelAll();
                    _materialMetadataService.Update(id, md => md.Name = name);
                }
                return success;
            }
        }

        public bool Delete(int id, out string? errmsg)
        {
            var m = _materialRepo.GetById(id);
            if (m is null)
            {
                errmsg = "找不到指定素材";
                return false;
            }
            string? oldPathName = m.StorePathName;
            if (oldPathName != null)
                _storage.Delete(oldPathName, out _);
            var success = _materialRepo.TryRemove(m, out errmsg);
            if (success)
            {
                _cacheExpTokenService.MaterialNamePathInfo.CancelAll();
                _materialMetadataService.Remove(id);
            }
            return success;
        }


        private const int materialMaxSide = 256;
        public static bool CompressImage(Stream input, MemoryStream output, out string? errmsg)
        {
            Image img;
            try
            {
                using MemoryStream inputData = new();
                input.CopyTo(inputData);
                inputData.Seek(0, SeekOrigin.Begin);
                var imgInfo = Image.Identify(inputData);
                if (imgInfo.Width > 2000 || imgInfo.Height > 2000)
                {
                    errmsg = "请勿上传长宽超过2000像素的图片";
                    return false;
                }
                inputData.Seek(0, SeekOrigin.Begin);
                img = Image.Load(inputData);
            }
            catch
            {
                errmsg = "无法识别图片格式";
                return false;
            }
            int w = img.Width;
            int h = img.Height;
            float ratio = (float)w / h;
            if (ratio > 4 || ratio < 0.25f)
            {
                errmsg = "请勿上传奇怪的长宽比";
                return false;
            }
            if (img.Size.Width <= materialMaxSide && img.Size.Height <= materialMaxSide) {
                img.SaveAsPng(output);
                img.Dispose();
                errmsg = null;
                return true;
            }
            Size newSize;
            if (w > h)
                newSize = new Size(materialMaxSide, (int)(materialMaxSide / ratio));
            else if (w < h)
                newSize = new Size((int)(materialMaxSide * ratio), materialMaxSide);
            else
                newSize = new Size(materialMaxSide, materialMaxSide);
            img.Mutate(x => x.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center,
                Size = newSize
            }));
            img.SaveAsPng(output);
            img.Dispose();
            errmsg = null;
            return true;
        }

        private static bool CanCompress(string fileName)
        {
            return fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg") || fileName.EndsWith(".png");
        }
        private static bool CanSaveButNoCompress(string fileName)
        {
            return fileName.EndsWith(".svg") || fileName.EndsWith(".gif");
        }
        private static string StorePathName(string path, string name) => path + "/" + name;

        private const int noCompressMaxSize = 500 * 1024;
        private const string noCompressMaxSizeExceedMsg = "svg或gif不能超过500KB";



        public string GetMaterialFullSrc(string pathName)
        {
            return _storage.FullUrl(pathName);
        }

        public class MaterialIndexItem
        {
            public MaterialIndexItem(Material m, Func<string, string> fullPath)
            {
                Id = m.Id;
                Name = m.Name ?? "??";
                Src = fullPath(m.StorePathName ?? "??");
                Desc = m.Desc ?? "";
                Time = m.Updated.ToString("yy/MM/dd HH:mm");
            }
            public int Id { get; }
            public string Name { get; }
            public string Src { get; }
            public string Desc { get; }
            public string Time { get; }
        }
    }
}
