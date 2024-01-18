using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Repos;
using FCloud3.Repos.Files;

namespace FCloud3.Services.Files
{
    public class FileDirService
    {
        private readonly FileDirRepo _fileDirRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly DbTransactionService _transaction;
        private readonly IFileItemService _fileItemService;

        public FileDirService(
            FileDirRepo fileDirRepo,
            FileItemRepo fileItemRepo,
            DbTransactionService transaction,
            IFileItemService fileItemService)
        {
            _fileDirRepo = fileDirRepo;
            _fileItemRepo = fileItemRepo;
            _transaction = transaction;
            _fileItemService = fileItemService;
        }

        public FileDir? GetById(int id)
        {
            return _fileDirRepo.GetById(id);
        }
        public FileDirIndexResult? GetSubDirAndItemsByPath(IndexQuery q, string[] path, out string? errmsg)
        {
            var subDirsQ = _fileDirRepo.GetChildrenByPath(path,out int thisDirId ,out errmsg);
            if(subDirsQ is null)
                return null;
            var subDirs = _fileDirRepo.IndexFilterOrder(subDirsQ,q);
            var subDirsData = subDirs.TakePage(q,x => new FileDirSubDir()
            {
                Id = x.Id,
                Name = x.Name,
                Updated = x.Updated.ToString("yy/MM/dd HH:mm"),
                ByteCount = x.ByteCount,
            });


            //两个查询共用一个IndexQuery，查询前翻译一下名字
            static string keyReplace(string k)
            {
                if (k == nameof(FileDir.Name))
                    return nameof(FileItem.DisplayName);
                return k;
            }
            IndexResult<FileDirItem>? itemsData = null;
            if (thisDirId != 0)
            {
                var itemsQ = _fileItemRepo.GetByDirId(thisDirId);
                var items = _fileItemRepo.IndexFilterOrder(itemsQ, q, keyReplace);

                itemsData = items.TakePage(q, x => new FileDirItem()
                {
                    Id = x.Id,
                    Name = x.DisplayName,
                    ByteCount = x.ByteCount,
                    Updated = x.Updated.ToString("yy/MM/dd HH:mm"),
                    Url = _fileItemService.Url(x.StorePathName ?? "missing")
                });
            }
            return new() { 
                Items = itemsData,
                SubDirs = subDirsData,
                ThisDirId = thisDirId
            };
        }
        public FileDirTakeContentResult TakeContent(int dirId)
        {
            var subDirs = _fileDirRepo.Existing
                .Where(x => x.ParentDir == dirId)
                .Select(x => new { x.Id, x.Name })
                .OrderBy(x => x.Name).ToList();
            var items = _fileItemRepo.Existing
                .Where(x => x.InDir == dirId)
                .Select(x => new { x.Id, x.DisplayName, x.ByteCount, x.StorePathName })
                .OrderBy(x=>x.DisplayName).ToList();
            FileDirTakeContentResult res = new();
            subDirs.ForEach(x =>
            {
                res.SubDirs.Add(new() { Id = x.Id, Name = x.Name });
            });
            items.ForEach(x =>
            {
                res.Items.Add(new()
                {
                    Id = x.Id,
                    Name = x.DisplayName,
                    Url = _fileItemService.Url(x.StorePathName??"missing"),
                    ByteCount = x.ByteCount
                });
            });
            return res;
        }

        public bool UpdateInfo(int id,string? name,out string? errmsg)
        {
            var target = _fileDirRepo.GetById(id);
            if (target is null)
            {
                errmsg = "找不到该文件夹";
                return false;
            }
            target.Name = name;
            if(!_fileDirRepo.TryEdit(target, out errmsg))
                return false;
            return true;
        }
        public bool PutInFile(string[] dirPath, int fileItemId, out string? errmsg)
        {
            errmsg = null;
            //TODO权限验证，文件是不是自己的，文件夹有没有放文件权限
            FileItem? f = _fileItemRepo.GetById(fileItemId);
            if(f is null)
            {
                errmsg = "未找到要放入的文件";
                return false;
            }
            //应该是从根文件夹全拿出来
            FileDir? dir = _fileDirRepo.GetByPath(dirPath);
            if(dir is null)
            {
                errmsg = "未找到目标文件夹";
                return false;
            }
            f.InDir = dir.Id;
            //TODO：上级文件夹也要更新这俩
            dir.ByteCount += f.ByteCount;
            dir.ContentCount += 1;
            string? transErrmsg = null;
            bool success = _transaction.DoTransaction(() =>
            {
                bool dirUpdateSuccess = _fileDirRepo.TryEdit(dir, out string? dirErr);
                bool fileUpdateSuccess = _fileItemRepo.TryEdit(f, out string? fileErr);
                if (dirUpdateSuccess && fileUpdateSuccess)
                    return true;
                transErrmsg = dirErr + fileErr;
                return false;
            });
            errmsg = transErrmsg;
            return success;
        }
    }

    public class FileDirSubDir
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Updated { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public int ByteCount { get; set; }
        public int FileNumber { get; set; }
    }
    public class FileDirItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Updated { get; set; }
        public int ByteCount { get; set; }
        public string? Url { get; set; }
    }
    public class FileDirIndexResult
    {
        public IndexResult<FileDirSubDir>? SubDirs { get; set; }
        public IndexResult<FileDirItem>? Items { get; set; }
        public int ThisDirId { get; set; }
    }
    public class FileDirTakeContentResult
    {
        public List<TakeContentResSubDir> SubDirs { get; set; }
        public List<TakeContentResItem> Items { get; set; }
        public class TakeContentResSubDir
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
        public class TakeContentResItem
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Update { get; set; }
            public int ByteCount { get; set; }
            public string? Url { get; set; }
        }
        public FileDirTakeContentResult()
        {
            SubDirs = new();
            Items = new();
        }
    }
}
