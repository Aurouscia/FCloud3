using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Messages;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc.Index;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Identities;

namespace FCloud3.Services.Files
{
    public class FileDirService
    {
        private readonly UserRepo _userRepo;
        private readonly int _userId;
        private readonly FileDirRepo _fileDirRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly WikiToDirRepo _wikiToDirRepo;
        private readonly OpRecordRepo _opRecordRepo;
        private readonly AuthGrantService _authGrantService;
        private readonly IStorage _storage;
        private readonly DbTransactionService _dbTransactionService;

        public FileDirService(
            UserRepo userRepo,
            IOperatingUserIdProvider userIdProvider,
            FileDirRepo fileDirRepo,
            FileItemRepo fileItemRepo,
            WikiItemRepo wikiItemRepo,
            WikiToDirRepo wikiToDirRepo,
            OpRecordRepo opRecordRepo,
            AuthGrantService authGrantService,
            IStorage storage,
            DbTransactionService dbTransactionService)
        {
            _userRepo = userRepo;
            _userId = userIdProvider.Get();
            _fileDirRepo = fileDirRepo;
            _fileItemRepo = fileItemRepo;
            _wikiItemRepo = wikiItemRepo;
            _wikiToDirRepo = wikiToDirRepo;
            _opRecordRepo = opRecordRepo;
            _authGrantService = authGrantService;
            _storage = storage;
            _dbTransactionService = dbTransactionService;
        }

        public FileDir? GetById(int id)
        {
            return _fileDirRepo.GetById(id);
        }

        public List<string>? GetPathById(int id)
        {
            return _fileDirRepo.GetPathById(id);
        }


        private enum FileDirContentItemType
        {
            Dir, WikiItem, FileItem
        }
        private struct FileDirContentItem(int id, int owner, FileDirContentItemType type, DateTime updated)
        {
            public int Id { get; } = id;
            public int Owner { get; } = owner;
            public FileDirContentItemType Type { get; } = type;
            public DateTime Updated { get; } = updated;
        }
        public FileDirIndexResult? GetContent(IndexQuery q, string[] path, bool isAdmin, out string? errmsg)
        {
            q.SelfCheck();
            if (path.Length == 1 || path.Length == 2)
            {
                if (path[0] == homelessItems)
                {
                    return GetHomelessItems(q, path, out errmsg);
                }
            }
            errmsg = null;
            var chain = _fileDirRepo.GetChainByPath(path);
            if (chain is null) {
                errmsg = "找不到指定路径的文件夹";
                return null;
            }
            List<string> friendlyPath = chain.ConvertAll(x => x.Name ?? "??").ToList();
            var thisDirId = 0;
            if (chain.Count > 0)
                thisDirId = chain.Last().Id;

            var ownerId = _fileDirRepo.GetOwnerIdById(thisDirId);
            var ownerName = "";
            Func<int, string> getUserName = x => _userRepo.CachedItemById(x)?.Name ?? "??";
            if (ownerId > 0)
                ownerName = getUserName(ownerId);


            List<FileDirContentItem> contents = [];
            static string keyReplaceForWikiItem(string k)
            {
                if (k == nameof(FileDir.Name))
                    return nameof(WikiItem.Title);
                return k;
            }
            static string keyReplaceForFileItem(string k)
            {
                if (k == nameof(FileDir.Name))
                    return nameof(FileItem.DisplayName);
                return k;
            }


            var subDirsQ = _fileDirRepo.GetChildrenById(thisDirId);
            if (subDirsQ is null)
                return null;
            subDirsQ = _fileDirRepo.IndexFilterOrder(subDirsQ, q);
            contents.AddRange(subDirsQ.Select(x => 
                new FileDirContentItem(x.Id, x.CreatorUserId, FileDirContentItemType.Dir, x.Updated)).ToList());

            if (thisDirId > 0)
            {
                var wikiFrom = isAdmin ? _wikiItemRepo.Existing : _wikiItemRepo.ExistingAndNotSealed;
                var wikisQ = from wiki in wikiFrom
                             from relation in _wikiToDirRepo.Existing
                             where relation.DirId == thisDirId
                             where wiki.Id == relation.WikiId
                             select wiki;
                wikisQ = _wikiItemRepo.IndexFilterOrder(wikisQ, q, keyReplaceForWikiItem);
                contents.AddRange(wikisQ.Select(x =>
                    new FileDirContentItem(x.Id, x.OwnerUserId, FileDirContentItemType.WikiItem, x.Updated)).ToList());
            

                var filesQ = _fileItemRepo.GetByDirId(thisDirId);
                filesQ = _fileItemRepo.IndexFilterOrder(filesQ, q, keyReplaceForFileItem);
                contents.AddRange(filesQ.Select(x => 
                    new FileDirContentItem(x.Id, x.CreatorUserId, FileDirContentItemType.FileItem, x.Updated)).ToList());
            }

            //默认排序规则
            if(q.OrderBy is null)
                contents.Sort((x, y) =>
                {
                    int xOwned = x.Owner == _userId ? 1 : 0;
                    int yOwned = y.Owner == _userId ? 1 : 0;
                    int owningDiff = yOwned - xOwned;
                    if (owningDiff != 0)
                        return owningDiff;
                    int typeDiff = x.Type - y.Type;
                    if (typeDiff != 0)
                        return typeDiff;
                    return DateTime.Compare(y.Updated, x.Updated);
                });
            var itemsPaged = contents.AsQueryable().TakePage(q, out int totalCount, out int pageIdx, out int pageCount).ToList();

            var subDirList = _fileDirRepo.GetRangeByIdsOrdered(
                ids: itemsPaged.Where(x => x.Type == FileDirContentItemType.Dir).Select(x => x.Id).ToList(),
                converter: x => FileDirIndexResult.FileDirSubDir.Converter(x, getUserName));
            var wikiList = _wikiItemRepo.GetRangeByIdsOrdered(
                ids: itemsPaged.Where(x => x.Type == FileDirContentItemType.WikiItem).Select(x => x.Id).ToList(),
                converter: x => FileDirIndexResult.FileDirWiki.Converter(x, getUserName));
            var fileList = _fileItemRepo.GetRangeByIdsOrdered(
                ids: itemsPaged.Where(x => x.Type == FileDirContentItemType.FileItem).Select(x => x.Id).ToList(),
                converter: x => FileDirIndexResult.FileDirItem.Converter(x, _storage.FullUrl, getUserName));

            var subDirData = new IndexResult<FileDirIndexResult.FileDirSubDir>(subDirList, 0, 0, 0);
            var wikiData = new IndexResult<FileDirIndexResult.FileDirWiki>(wikiList, 0, 0, 0);
            var fileData = new IndexResult<FileDirIndexResult.FileDirItem>(fileList, 0, 0, 0);

            //subDirs会被用来显示页数
            subDirData.PageCount = pageCount;
            subDirData.TotalCount = totalCount;
            subDirData.PageIdx = pageIdx;

            return new() { 
                Items = fileData,
                SubDirs = subDirData,
                Wikis = wikiData,
                ThisDirId = thisDirId,
                OwnerId = ownerId,
                OwnerName = ownerName,
                FriendlyPath = friendlyPath
            };
        }
        public FileDirIndexResult? GetHomelessItems(IndexQuery q, string[] path, out string? errmsg)
        {
            string? userName = null;
            if (path.Length == 0 || path.Length > 2 || path[0] != homelessItems)
            {
                errmsg = "内部错误：GetHomelessItems错误调用";
                return null;
            }
            if (path.Length == 2)
                userName = path[1];
            var userId = 0;
            if (!string.IsNullOrEmpty(userName))
                userId = _userRepo.CachedItemByPred(x=>x.Name == userName)?.Id ?? 0;
            if (userId == 0) 
            {
                userId = _userId;
                userName = _userRepo.Existing.Where(x => x.Id == userId).Select(x => x.Name).FirstOrDefault();
            }
            if (userId == 0 || userName is null)
            {
                errmsg = "找不到指定用户";
                return null;
            }

            var homelessFiles =
                from f in _fileItemRepo.Existing
                where f.InDir == 0
                where f.CreatorUserId == userId
                select f;
            static string keyReplaceForFileItem(string k)
            {
                if (k == nameof(FileDir.Name))
                    return nameof(FileItem.DisplayName);
                return k;
            }
            var items = _fileItemRepo.IndexFilterOrder(homelessFiles, q, keyReplaceForFileItem);
            IndexResult<FileDirIndexResult.FileDirItem>? itemsData = null;
            itemsData = items.TakePageAndConvertOneByOne(q, x => 
                new FileDirIndexResult.FileDirItem(x.Id,x.DisplayName,x.Updated,userName,x.ByteCount, _storage.FullUrl(x.StorePathName ?? "??"))
            );

            FileDirIndexResult res = new()
            {
                Wikis = new(new(),1,1,1),
                Items = itemsData,
                SubDirs = new(new(),1,1,1),
                FriendlyPath = new List<string> { $"无归属文件(属于 {userName})" },
                ThisDirId = -1
            };
            
            //subDirs会被用来显示页数
            res.SubDirs.PageCount = itemsData.PageCount;
            res.SubDirs.PageIdx = itemsData.PageIdx;
            res.SubDirs.TotalCount = itemsData.TotalCount;
            errmsg = null;
            return res;
        }

        public bool UpdateInfo(int id, string? name, string? urlPathName, out string? errmsg)
        {
            var target = _fileDirRepo.GetById(id);
            if (target is null)
            {
                errmsg = "找不到该文件夹";
                return false;
            }
            if(string.IsNullOrEmpty(urlPathName))
            {
                errmsg = "文件夹路径名不能为空";
                return false;
            }
            if (PreservedUrlPathNames().Contains(urlPathName))
            {
                errmsg = $"请勿使用保留文件夹名[{urlPathName}]";
                return false;
            }

            string record = "";
            if (target.Name != name)
                record += $"将 {target.Name} 更名为 {name} ; ";
            if (target.UrlPathName != urlPathName)
                record += $"将路径名 {target.UrlPathName} 改为 {urlPathName}";

            target.Name = name;
            target.UrlPathName = urlPathName;

            if(!_fileDirRepo.TryUpdate(target, out errmsg))
                return false;

            if (!string.IsNullOrEmpty(record))
                _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.FileDir, id, 0, record);
            return true;
        }

        public List<int>? MoveFilesIn(int distDirId, List<int> fileItemIds, bool bypassAuth,out string? failMsg, out string? errmsg)
        {
            failMsg = null;
            if (fileItemIds.Count > 5)
            {
                errmsg = "太多了，请一个个移";
                return null;
            }
            int originalCount = fileItemIds.Count;
            if(!bypassAuth)
                fileItemIds.RemoveAll(x => !_authGrantService.CheckAccess(AuthGrantOn.FileItem, x));
            if (originalCount > fileItemIds.Count)
                failMsg = "无权移动该文件，请咨询管理员";
            if (distDirId < 0)
            {
                errmsg = "请刷新后重试(未找到指定路径的文件夹)";
                return null;
            }
            if(distDirId == 0)
            {
                errmsg = "不能将文件放到根目录";
                return null;
            }

            if (fileItemIds.Count >= 1)
            {
                if (!_fileItemRepo.SetInDirForRange(distDirId, fileItemIds, out errmsg))
                    return null;
            }

            errmsg = null;
            return fileItemIds;
        }
        public List<int>? MoveDirsIn(int distDirId, List<int> fileDirIds, bool bypassAuth, out string? failMsg, out string? errmsg)
        {
            failMsg = null;
            var dist = _fileDirRepo.GetById(distDirId);
            if (dist is null && distDirId != 0)
            {
                errmsg = "未找到目的地文件夹";
                return null; 
            }

            if (fileDirIds.Count > 5)
            {
                errmsg = "太多了，请一个个移";
                return null;
            }
            int originalCount = fileDirIds.Count;
            if(!bypassAuth)
                fileDirIds.RemoveAll(x => !_authGrantService.CheckAccess(AuthGrantOn.Dir, x));
            if (originalCount > fileDirIds.Count)
                failMsg = "无权移动该目录，请咨询管理员";

            if (fileDirIds.Count == 0)
            {
                errmsg = null;
                return null;
            }

            errmsg = null;
            var ds = _fileDirRepo.GetRangeByIds(fileDirIds).ToList();
            var pathNames = ds.Select(x => x.UrlPathName).ToList();
            if(pathNames.Distinct().Count() < pathNames.Count)
            {
                errmsg = $"试图移入多个路径名相同的目录";
                return null;
            }

            var setDepth = dist is null ? 0 : dist.Depth+1; //如果父目录是0，那么深度设为0，否则设为父目录深度+1
            int? setRoot = dist is null ? null : dist.RootDir; //如果父目录是0，那么root设为自己的id，否则设为父目录的root

            using var transaction = _dbTransactionService.BeginTransaction();
            try
            {
                ds.ForEach(x =>
                {
                    x.Depth = setDepth;
                    x.ParentDir = distDirId;
                    x.RootDir = setRoot ?? x.Id;
                });
                if (!_fileDirRepo.TryUpdateParentForRange(distDirId, ds, out errmsg))
                    throw new Exception(errmsg);
                if (!_fileDirRepo.UpdateDescendantsInfoFor(fileDirIds, out errmsg))
                    throw new Exception(errmsg);
            }
            catch
            {
                //发生任何错误，回滚数据库事务，避免数据不一致
                transaction.Rollback();
                return [];
            }
            //没有错误 提交事务
            transaction.Commit();
            return fileDirIds;
        }
        public List<int>? MoveWikisIn(int distDirId, List<int> wikiItemIds, bool bypassAuth, out string? failMsg, out string? errmsg)
        {
            failMsg = null;
            if (wikiItemIds.Count > 5)
            {
                errmsg = "太多了，请一个个移";
                return null;
            }
            int originalCount = wikiItemIds.Count;
            if(!bypassAuth)
                wikiItemIds.RemoveAll(x => !_authGrantService.CheckAccess(AuthGrantOn.WikiItem, x));
            if (originalCount > wikiItemIds.Count)
                failMsg = "无权移动该词条，请咨询管理员";
            if (wikiItemIds.Count > 0)
            {
                if (!_wikiToDirRepo.AddWikisToDir(wikiItemIds, distDirId, out errmsg))
                    return null;
            }
            errmsg = null;
            return wikiItemIds;
        }


        public FileDirPutInResult? MoveThingsIn(int dirId, List<int>? fileItemIds, List<int>? fileDirIds, List<int>? wikiItemIds, bool bypassAuth, out string? errmsg)
        {
            List<int>? chain = _fileDirRepo.GetChainIdsById(dirId);
            return MoveThingsIn(chain, fileItemIds, fileDirIds, wikiItemIds, bypassAuth, out errmsg);
        }
        private FileDirPutInResult? MoveThingsIn(List<int>? dirIdsChain, List<int>? fileItemIds, List<int>? fileDirIds, List<int>? wikiItemIds, bool bypassAuth, out string? errmsg)
        {
            errmsg = null;
            string? failMsg = null;
            bool didSth = false;
            List<int>? fileItemSuccess = null;
            List<int>? fileDirSuccess = null;
            List<int>? wikiItemSuccess = null;

            if (dirIdsChain is null) { errmsg = "找不到指定路径的文件夹"; return null; }

            int distDirId = dirIdsChain.Count > 0 ? dirIdsChain.Last() : 0;

            if (fileDirIds is not null && fileDirIds.Count > 0)
            {
                if (fileDirIds.Any(x => dirIdsChain.Contains(x)))
                {
                    errmsg = "检测到循环，请勿将文件夹移入自身或子级";
                    return null;
                }
                fileDirSuccess = MoveDirsIn(distDirId, fileDirIds, bypassAuth, out failMsg, out errmsg);
                if (errmsg is not null)
                    return null;
                didSth = true;
            }
            if (fileItemIds is not null && fileItemIds.Count > 0)
            {
                fileItemSuccess = MoveFilesIn(distDirId, fileItemIds, bypassAuth, out failMsg, out errmsg);
                if (errmsg is not null)
                    return null;
                didSth = true;
            }
            if (wikiItemIds is not null && wikiItemIds.Count > 0)
            {
                wikiItemSuccess = MoveWikisIn(distDirId, wikiItemIds, bypassAuth, out failMsg, out errmsg);
                if (errmsg is not null)
                    return null;
                didSth = true;
            }
            if (!didSth)
            {
                errmsg = "未选择任何要移入的对象";
                return null;
            }

            _fileDirRepo.UpdateTime(dirIdsChain);
            var resp = new FileDirPutInResult()
            {
                FileItemSuccess = fileItemSuccess,
                FileDirSuccess = fileDirSuccess,
                WikiItemSuccess = wikiItemSuccess,

                FailMsg = failMsg,
            };
            return resp;
        }
        public bool MoveFileIn(int distDirId, int fileItemId, bool bypassAuth, out string? errmsg)
        {
            var list = new List<int>() { fileItemId };
            _ = MoveFilesIn(distDirId, list, bypassAuth, out string? failMsg, out errmsg);
            if (errmsg is null && failMsg is not null)
                errmsg = failMsg;
            if (errmsg is not null)
                return false;
            _fileDirRepo.SetUpdateTimeAncestrally(distDirId, out _);
            return true;
        }
        public bool Create(int parentDir, string? name, string? urlPathName, out string? errmsg)
        {
            FileDir? parent = _fileDirRepo.GetById(parentDir);
            if(parent is null && parentDir != 0)
            {
                errmsg = "找不到指定父文件夹";
                return false;
            }

            int depth = 0;
            if (parent is not null)
                depth = parent.Depth + 1;
            int rootDir = 0;
            if (parent is not null)
                rootDir = parent.RootDir;
            FileDir newDir = new()
            {
                ParentDir = parentDir,
                Name = name,
                UrlPathName = urlPathName,
                Depth = depth,
                RootDir = rootDir
            };
            int created = _fileDirRepo.TryAddAndGetId(newDir, out errmsg);
            if (created > 0)
            {
                string parentName = parent?.Name ?? "根目录";
                _fileDirRepo.SetUpdateTimeAncestrally(parentDir, out errmsg);
                _opRecordRepo.Record(OpRecordOpType.Create, OpRecordTargetType.FileDir, created, parentDir,
                    $"在 {parentName} 中新建目录 {name} ({urlPathName})");
                return true;
            }
            return false;
        }
        public bool Delete(int dirId, out string? errmsg)
        {
            errmsg = null;
            var item = _fileDirRepo.GetById(dirId);
            if (item is null)
                return false;
            var items = _fileItemRepo.GetByDirId(dirId).Count();
            if (items > 0) 
            {
                errmsg = "只能删除空文件夹";
                return false;
            }
            //词条被删除，词条-目录关系仍残留，需要检查所有关系指向的存在的词条数量
            var wikis =
                from relation in _wikiToDirRepo.Existing
                from w in _wikiItemRepo.Existing
                where relation.DirId == dirId
                where relation.WikiId == w.Id
                select w.Id;
            if (wikis.Count() > 0)
            {
                errmsg = "只能删除空文件夹";
                return false;
            }
            var subDirs = _fileDirRepo.GetChildrenById(dirId)?.Count() ?? 0;
            if (subDirs > 0)
            {
                errmsg = "只能删除空文件夹";
                return false;
            }
            if(_fileDirRepo.TryRemove(item,out errmsg))
            {
                string? parentName = null;
                if (item.ParentDir > 0)
                    parentName = _fileDirRepo.GetqById(item.ParentDir).Select(x => x.Name).FirstOrDefault();
                parentName ??= "根目录";
                _opRecordRepo.Record(OpRecordOpType.Remove, OpRecordTargetType.FileDir, dirId, 0, 
                    $"从 {parentName} 中删除目录 {item.Name} ({item.UrlPathName})");
                return true;
            }
            return false;
        }

        public bool ManualFixInfoForAll(out string? errmsg) 
            => _fileDirRepo.ManualFixInfoForAll(out errmsg);
        public List<int> ManualLoopFix()
            => _fileDirRepo.ManualLoopFix();

        private const string homelessItems = "homeless-items";
        private static List<string> PreservedUrlPathNames()
        {
            return new List<string> { homelessItems };
        } 
    }

    public class FileDirIndexResult
    {
        public IndexResult<FileDirSubDir>? SubDirs { get; set; }
        public IndexResult<FileDirItem>? Items { get; set; }
        public IndexResult<FileDirWiki>? Wikis { get; set; }
        public int ThisDirId { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public List<string>? FriendlyPath { get; set; }

        public class FileDirSubDir(int id, string? name, string? urlPathName, DateTime updated, string ownerName)
        {
            public int Id { get; set; } = id;
            public string? Name { get; set; } = name;
            public string? UrlPathName { get; set; } = urlPathName;
            public string? Updated { get; set; } = updated.ToString("yy-MM-dd HH:mm");
            public string? OwnerName { get; set; } = ownerName;
            public int ByteCount { get; set; } = 0;
            public int FileNumber { get; set; } = 0;

            public static Dictionary<int, FileDirSubDir> Converter(
                IQueryable<FileDir> fileDirs, Func<int, string> getUserName)
            {
                var data = fileDirs.Select(x 
                    => new { x.Id, x.Name, x.UrlPathName, x.Updated, x.CreatorUserId })
                    .ToList()
                    .ConvertAll(x
                        =>new FileDirSubDir(x.Id, x.Name,x.UrlPathName,x.Updated,getUserName(x.CreatorUserId)));
                return data.ToDictionary(x => x.Id, x => x);
            }
        }
        public class FileDirItem(int id, string? name, DateTime updated, string ownerName, int byteCount, string? url)
        {
            public int Id { get; set; } = id;
            public string? Name { get; set; } = name;
            public string? Updated { get; set; } = updated.ToString("yy-MM-dd HH:mm");
            public string? OwnerName { get; set; } = ownerName;
            public int ByteCount { get; set; } = byteCount;
            public string? Url { get; set; } = url;

            public static Dictionary<int, FileDirItem> Converter(
                IQueryable<FileItem> files, Func<string, string> url, Func<int, string> getUserName)
            {
                var data = files.Select(x 
                    => new{
                        x.Id,
                        x.DisplayName,
                        x.Updated,
                        x.CreatorUserId,
                        x.ByteCount,
                        x.StorePathName}
                ).ToList().ConvertAll(x=>new FileDirItem(
                    x.Id, 
                    x.DisplayName,
                    x.Updated,
                    getUserName(x.CreatorUserId), 
                    x.ByteCount,
                    url(x.StorePathName??"??")
                    ));
                return data.ToDictionary(x => x.Id, x => x);
            }
        }
        public class FileDirWiki(int id, string? name, string? urlPathName, DateTime updated, string ownerName)
        {
            public int Id { get; set; } = id;
            public string? Name { get; set; } = name;
            public string? UrlPathName { get; set; } = urlPathName;
            public string? Updated { get; set; } = updated.ToString("yy-MM-dd HH:mm");
            public string? OwnerName { get; set; } = ownerName;
            public static Dictionary<int, FileDirWiki> Converter(
                IQueryable<WikiItem> wikis, Func<int, string> getUserName)
            {
                var data = wikis.Select(x 
                    => new {x.Id, x.Title, x.UrlPathName, x.Updated, x.OwnerUserId})
                    .ToList()
                    .ConvertAll(x 
                        => new FileDirWiki(x.Id, x.Title, x.UrlPathName, x.Updated, getUserName(x.OwnerUserId)));
                return data.ToDictionary(x => x.Id, x => x);
            }
        }
    }
    public class FileDirPutInResult
    {
        public List<int>? FileItemSuccess { get; set; }
        public List<int>? FileDirSuccess { get; set; }
        public List<int>? WikiItemSuccess { get; set; }
        public string? FailMsg { get; set; }
    }
}
