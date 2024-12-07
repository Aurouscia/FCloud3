using FCloud3.Entities.Files;
using FCloud3.Repos.Etc.Index;
using FCloud3.Repos.Files;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Wiki;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FCloud3.Services.Files
{
    public class MaterialService(
        MaterialRepo materialRepo,
        WikiRefService wikiRefService,
        IOperatingUserIdProvider userIdProvider,
        IStorage storage)
    { 

        public IndexResult<MaterialIndexItem> Index(IndexQuery q, bool onlyMine)
        {
            bool anySearch = q.Search is { } && q.Search.Count > 0;
            IQueryable<Material> from = anySearch ? materialRepo.Existing : materialRepo.ExistingExceptHidden;
            if (onlyMine)
            {
                var userId = userIdProvider.Get();
                from = from.Where(x => x.CreatorUserId == userId);
            }
            var m = materialRepo.IndexFilterOrder(from, q)
                .TakePageAndConvertOneByOne(q, x => new MaterialIndexItem(x, storage.FullUrl));
            return m;
        }

        public int Add(Stream stream, string formFileName, string path, string? name, string? desc, out string? errmsg)
        {
            Material m = new()
            {
                Name = name,
                StorePathName = null,
                Desc = desc
            };
            //进行名称检查
            if (!materialRepo.ModelCheck(m, out errmsg))
                return 0;

            using MemoryStream compressed = new();
            string ext;
            if (CanCompress(formFileName))
            {
                if (!CompressImage(stream, compressed, out errmsg))
                    return 0;
                ext = ".png";
            }
            else if (CanSaveButNoCompress(formFileName))
            {
                stream.CopyTo(compressed);
                if (compressed.Length > noCompressMaxSize)
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
            if (!storage.Save(compressed, storePathName, out errmsg))
                return 0;

            //写入存储名
            m.StorePathName = storePathName;
            var createdId = materialRepo.TryAddAndGetId(m, out errmsg);
            wikiRefService.ReferencedMaterialPropChangeHandle(name);
            return createdId;
        }

        public bool UpdateContent(int id, Stream stream, string formFileName, string path, out string? errmsg)
        {
            Material? m = materialRepo.GetById(id);
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
            if (!storage.Save(compressed, storePathName, out errmsg))
                return false;
            string? oldPathName = m.StorePathName;
            if (oldPathName != null)
                storage.Delete(oldPathName, out _);
            m.StorePathName = storePathName;
            materialRepo.UpdateInfoWithoutCheck(m);
            wikiRefService.ReferencedMaterialPropChangeHandle(m.Name);
            return true;
        }

        public bool UpdateInfo(int id, string name, string? desc, out string? errmsg)
        {
            var m = materialRepo.GetById(id);
            if (m is null)
            {
                errmsg = "找不到指定素材";
                return false;
            }
            string? originalName = m.Name;
            m.Name = name;
            m.Desc = desc;
            if (!materialRepo.TryUpdateInfo(m, out errmsg))
                return false;
            if(originalName != name)
                wikiRefService.ReferencedMaterialPropChangeHandle(originalName, name);
            errmsg = null;
            return true;
        }
    

        public bool Delete(int id, out string? errmsg)
        {
            var m = materialRepo.GetById(id);
            if (m is null)
            {
                errmsg = "找不到指定素材";
                return false;
            }
            string? oldPathName = m.StorePathName;
            if (oldPathName != null)
                storage.Delete(oldPathName, out _);
            materialRepo.Remove(m);
            wikiRefService.ReferencedMaterialPropChangeHandle(m.Name);
            errmsg = null;
            return true;
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
            return storage.FullUrl(pathName);
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
