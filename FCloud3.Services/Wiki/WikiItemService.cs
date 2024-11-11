using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Files;
using FCloud3.Entities.Messages;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc.Index;
using FCloud3.Repos.Files;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Wiki.Support;
using System.Text;
using FCloud3.Repos.Identities;
using FCloud3.Services.Etc.TempData.EditLock;
using FCloud3.Services.Etc;
using NPOI.SS.Formula.PTG;

namespace FCloud3.Services.Wiki
{
    public class WikiItemService(
        DbTransactionService transaction,
        WikiItemRepo wikiRepo,
        WikiToDirRepo wikiToDirRepo,
        WikiParaRepo paraRepo,
        WikiTitleContainRepo wikiTitleContainRepo,
        TextSectionRepo textSectionRepo,
        FileItemRepo fileItemRepo,
        FileDirRepo fileDirRepo,
        FreeTableRepo freeTableRepo,
        OpRecordRepo opRecordRepo,
        UserRepo userRepo,
        ContentEditLockService contentEditLockService,
        LatestWikiExchangeService latestWikiExchangeService,
        WikiRefService wikiRefService,
        IOperatingUserIdProvider operatingUserIdProvider,
        IStorage storage)
    {
        private readonly DbTransactionService _transaction = transaction;
        private readonly WikiItemRepo _wikiRepo = wikiRepo;
        private readonly WikiToDirRepo _wikiToDirRepo = wikiToDirRepo;
        private readonly WikiParaRepo _paraRepo = paraRepo;
        private readonly WikiTitleContainRepo _wikiTitleContainRepo = wikiTitleContainRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FileItemRepo _fileItemRepo = fileItemRepo;
        private readonly FileDirRepo _fileDirRepo = fileDirRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;
        private readonly OpRecordRepo _opRecordRepo = opRecordRepo;
        private readonly UserRepo _userRepo = userRepo;
        private readonly ContentEditLockService _contentEditLockService = contentEditLockService;
        private readonly IOperatingUserIdProvider _operatingUserIdProvider = operatingUserIdProvider;
        private readonly IStorage _storage = storage;
        public const int maxWikiTitleLength = 30;

        public WikiItemCacheModel? GetInfoById(int id)
        {
            return _wikiRepo.CachedItemById(id);
        }
        public IndexResult<WikiItemIndexItem> Index(IndexQuery query)
        {
            return _wikiRepo.IndexFilterOrder(query).TakePageAndConvertOneByOne(query,x=>new WikiItemIndexItem(x));
        }

        /// <summary>
        /// 获取某wiki的一部分或全部段落
        /// </summary>
        /// <param name="wikiId">wiki的Id</param>
        /// <param name="start">从第几段开始（0是第一段）</param>
        /// <param name="count">取几段</param>
        /// <returns></returns>
        public List<WikiPara> GetWikiParas(int wikiId, int start = 0, int count = int.MaxValue)
        {
            var paras = _paraRepo.Existing
                .Where(x => x.WikiItemId == wikiId)
                .OrderBy(x => x.Order)
                .Skip(start)
                .Take(count)
                .ToList();
            return paras;
        }
        /// <summary>
        /// 获取某wiki的一部分或全部段落，并转换为显示用的数据
        /// </summary>
        /// <param name="wikiId">wiki的Id</param>
        /// <param name="start">从第几段开始（0是第一段）</param>
        /// <param name="count">取几段</param>
        /// <returns></returns>
        public List<WikiParaDisplay> GetWikiParaDisplays(int wikiId, int start = 0, int count = int.MaxValue)
        {
            if (!_wikiRepo.Existing.Any(x => x.Id == wikiId))
            {
                throw new Exception("找不到指定id的wiki");
            }
            var paras = GetWikiParas(wikiId, start, count);
            paras.EnsureOrderDense();

            List<int> textIds = paras.Where(x => x.Type == WikiParaType.Text).Select(x=>x.ObjectId).ToList();
            List<TextSectionMeta> textParaObjs = _textSectionRepo.GetMetaRangeByIds(textIds);

            List<int> fileIds = paras.Where(x => x.Type == WikiParaType.File).Select(x => x.ObjectId).ToList();
            List<FileItem> fileParaObjs = _fileItemRepo.GetRangeByIds(fileIds).ToList();

            List<int> tableIds = paras.Where(x => x.Type == WikiParaType.Table).Select(x => x.ObjectId).ToList();
            List<FreeTableMeta> tableParaObjs = _freeTableRepo.GetMetaRangeByIds(tableIds);

            var textContains = _wikiTitleContainRepo.GetByTypeAndObjIds(WikiParaType.Text, textIds);
            var tableContains = _wikiTitleContainRepo.GetByTypeAndObjIds(WikiParaType.Table, tableIds);

            List<WikiParaDisplay> paraObjs = paras.ConvertAll(x =>
            {
                WikiParaType type = x.Type;
                WikiParaDisplay? paraDisplay = null;
                if (type == WikiParaType.Text)
                {
                    var obj = textParaObjs.Find(p => p.Id == x.ObjectId);
                    var itsContainsCount = textContains.Count(c => c.ObjectId == x.ObjectId);
                    if(obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Title,
                            obj.ContentBrief, x.NameOverride, WikiParaType.Text, 0, itsContainsCount);
                }
                else if(type == WikiParaType.File)
                {
                    var obj = fileParaObjs.Find(p => p.Id == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.DisplayName,
                            _storage.FullUrl(obj.StorePathName??"missing"), x.NameOverride, WikiParaType.File, obj.ByteCount);
                }
                else if(type == WikiParaType.Table)
                {
                    var obj = tableParaObjs.Find(p => p.Id == x.ObjectId);
                    var itsContainsCount = tableContains.Count(c => c.ObjectId == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Name,
                            obj.Brief, x.NameOverride, WikiParaType.Table, 0, itsContainsCount);
                }
                paraDisplay ??= new WikiParaPlaceholder(type).ToDisplay(x);
                return paraDisplay;
            });
            return paraObjs;
        }
        public List<WikiParaDisplay>? GetWikiParaContents(int wikiId, out string? errmsg, int start = 0, int count = int.MaxValue)
        {
            if (!_wikiRepo.Existing.Any(x => x.Id == wikiId))
            {
                errmsg = "找不到指定id的wiki";
                return null;
            }
            var paras = GetWikiParas(wikiId, start, count);
            paras.EnsureOrderDense();
            
            List<(HeartbeatObjType type, int objId)> heartBeats = [];
            foreach (var p in paras)
            {
                if (p.Type == WikiParaType.Text)
                    heartBeats.Add((HeartbeatObjType.TextSection, p.ObjectId));
                else if (p.Type == WikiParaType.Table)
                    heartBeats.Add((HeartbeatObjType.FreeTable, p.ObjectId));
            }
            if (!_contentEditLockService.HeartbeatRange(heartBeats, true, out errmsg))
                return null;

            List<int> textIds = paras.Where(x => x.Type == WikiParaType.Text).Select(x => x.ObjectId).ToList();
            var textParaObjs = _textSectionRepo.GetRangeByIds(textIds).ToList();

            List<int> fileIds = paras.Where(x => x.Type == WikiParaType.File).Select(x => x.ObjectId).ToList();
            var fileParaObjs = _fileItemRepo.GetRangeByIds(fileIds).ToList();

            List<int> tableIds = paras.Where(x => x.Type == WikiParaType.Table).Select(x => x.ObjectId).ToList();
            var tableParaObjs = _freeTableRepo.GetRangeByIds(tableIds).ToList();
            
            var textContains = _wikiTitleContainRepo.GetByTypeAndObjIds(WikiParaType.Text, textIds);
            var tableContains = _wikiTitleContainRepo.GetByTypeAndObjIds(WikiParaType.Table, tableIds);
            
            List<WikiParaDisplay> paraObjs = paras.ConvertAll(x =>
            {
                WikiParaType type = x.Type;
                WikiParaDisplay? paraDisplay = null;
                if (type == WikiParaType.Text)
                {
                    var obj = textParaObjs.Find(p => p.Id == x.ObjectId);
                    var itsContainsCount = textContains.Count(c => c.ObjectId == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Title,
                            obj.Content, x.NameOverride, WikiParaType.Text, 0, itsContainsCount);
                }
                else if (type == WikiParaType.File)
                {
                    var obj = fileParaObjs.Find(p => p.Id == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.DisplayName,
                            _storage.FullUrl(obj.StorePathName ?? "missing"), x.NameOverride, WikiParaType.File, obj.ByteCount);
                }
                else if (type == WikiParaType.Table)
                {
                    var itsContainsCount = textContains.Count(c => c.ObjectId == x.ObjectId);
                    var obj = tableParaObjs.Find(p => p.Id == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Name,
                            obj.Data, x.NameOverride, WikiParaType.Table, 0, itsContainsCount);
                }
                paraDisplay ??= new WikiParaPlaceholder(type).ToDisplay(x);
                return paraDisplay;
            });
            return paraObjs;
        }
        public int InsertPara(int wikiId, int afterOrder, WikiParaType type, out string? errmsg)
        {
            string? msg = null;
            int newlyCreatedParaId = 0;
            bool success = _transaction.DoTransaction(() =>
            {
                var itsParas = GetWikiParas(wikiId);
                itsParas.EnsureOrderDense();
                var moveBackwards = itsParas.FindAll(x => x.Order > afterOrder);
                moveBackwards.ForEach(x => x.Order++);
                
                var underlyingId = 0;
                if (type == WikiParaType.Text)
                {
                    underlyingId = _textSectionRepo.AddDefaultAndGetId();
                }
                else if (type == WikiParaType.Table)
                {
                    underlyingId = _freeTableRepo.AddDefaultAndGetId();
                }
                WikiPara p = new()
                {
                    WikiItemId = wikiId,
                    Order = afterOrder+1,
                    Type = type,
                    ObjectId = underlyingId
                };
                newlyCreatedParaId = _paraRepo.AddAndGetId(p);
                if (newlyCreatedParaId <= 0)
                {
                    msg = "未知错误，段落创建失败";
                    return false;
                }
                _paraRepo.UpdateRange(itsParas);
                return true;
            });
            errmsg = msg;
            if (success)
            {
                SetWikiUpdated(wikiId);
                var w = _wikiRepo.CachedItemById(wikiId);
                _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem, wikiId, newlyCreatedParaId,
                    $"为 {w?.Title} ({w?.UrlPathName}) 在第 {afterOrder+1} 段后 插入了新 {WikiParaTypes.Readable(type)} 段落");
                return newlyCreatedParaId;
            }
            else
                return 0;
        }
        public bool SetParaOrders(int wikiId, List<int> orderedParaIds, out string? errmsg)
        {
            var itsParas = GetWikiParas(wikiId);
            itsParas.EnsureOrderDense();
            if (orderedParaIds.Count != itsParas.Count)
            {
                errmsg = "数据不一致，请刷新页面后重试";
                return false;
            }
            List<int> orderRecord = [];
            List<WikiPara> orderedParas = new(itsParas.Count);
            foreach (int id in orderedParaIds)
            {
                var p = itsParas.Find(x => x.Id == id);
                if (p is null)
                {
                    errmsg = "数据不一致，请刷新页面后重试";
                    return false;
                }
                orderedParas.Add(p);
                orderRecord.Add(p.Order + 1);
            }
            orderedParas.ResetOrder();
            _paraRepo.UpdateRange(orderedParas);

            SetWikiUpdated(wikiId);
            var name = _wikiRepo.CachedItemById(wikiId)?.Title;
            var orderRecordStr = string.Join('-', orderRecord);
            _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem, wikiId, 0,
                $"为 {name} 调整段落顺序为 {orderRecordStr}");
            errmsg = null;
            return true;
        }
        public bool RemovePara(int id, int paraId, out string? errmsg)
        {
            var paras = GetWikiParas(id);
            var target = paras.Find(x => x.Id == paraId);
            if (target is null)
            {
                errmsg = "未找到指定Id的目标段落";
                return false;
            }
            paras.Remove(target);
            paras.EnsureOrderDense();

            using var t = _transaction.BeginTransaction();
            _paraRepo.Remove(target);
            _paraRepo.UpdateRange(paras);
            t.Commit();

            SetWikiUpdated(id);
            var name = _wikiRepo.CachedItemById(id)?.Title;
            _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem, id, paraId,
                $"从 {name} 移除了第 {target.Order+1} 个段落({WikiParaTypes.Readable(target.Type)})");
            errmsg = null;
            return true;
        }

        public WikiInDirLocationView ViewDirLocations(string urlPathName)
        {
            var dirIds = (
                from w in _wikiRepo.Existing
                from r in _wikiToDirRepo.Existing
                where w.UrlPathName == urlPathName
                where r.WikiId == w.Id
                select r.DirId).ToList();

            var chains = _fileDirRepo.GetNameChainsByIds(dirIds);
            chains.Sort((x, y) =>
            {
                for(int i = 0; i < x.nameChain.Count && i < y.nameChain.Count; i++)
                {
                    var xn = x.nameChain[i];
                    var yn = y.nameChain[i];
                    if (xn != yn)
                        return string.Compare(xn, yn, StringComparison.CurrentCulture);
                }
                return x.nameChain.Count - y.nameChain.Count;
            });
            WikiInDirLocationView model = new();
            StringBuilder sb = new();
            chains.ForEach(x => model.Locations.Add(new(x.id, x.nameChain, sb)));

            var wiki = _wikiRepo.CachedItemByPred(x=>x.UrlPathName == urlPathName);
            if (wiki is not null)
            {
                model.WikiId = wiki.Id;
                model.Title = wiki.Title ?? "??";
            }
            return model;
        }
        public bool CreateInDir(string title,string urlPathName,int dirId, out string? errmsg)
        {
            var dir = _fileDirRepo.GetById(dirId);
            if (dir is null)
            {
                errmsg = "找不到指定目录";
                return false;
            }
            var newWiki = new WikiItem()
            {
                Title = title,
                UrlPathName = urlPathName,
            };
            int createdWikiId = _wikiRepo.TryAddAndGetId(newWiki, out errmsg);
            if (createdWikiId > 0)
            {
                wikiRefService.ReferencedWikiPropChangeHandle(createdWikiId, title, urlPathName);
                _opRecordRepo.Record(OpRecordOpType.Create, OpRecordTargetType.WikiItem, createdWikiId, 0, $"{title} ({urlPathName})");
                if(_wikiToDirRepo.AddWikisToDir([createdWikiId], dirId, out errmsg))
                {
                    _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.FileDir, dirId, createdWikiId,
                        $"将词条 {title} ({urlPathName}) 移入目录 {dir.Name}");
                    return true;
                }
            }
            return false;
        }
        public bool Create(string title, string urlPathName, out string? errmsg)
        {
            var newWiki = new WikiItem()
            {
                Title = title,
                UrlPathName = urlPathName,
            };
            var createdWikiId = _wikiRepo.TryAddAndGetId(newWiki, out errmsg);
            if (createdWikiId > 0)
            {
                wikiRefService.ReferencedWikiPropChangeHandle(createdWikiId, title, urlPathName);
                _opRecordRepo.Record(OpRecordOpType.Create, OpRecordTargetType.WikiItem, createdWikiId, 0, $"{title} ({urlPathName})");
                return true;
            }
            return false;
        }
        public bool RemoveFromDir(int wikiId, int dirId, out string? errmsg)
        {
            var dir = _fileDirRepo.GetById(dirId);
            if (dir is null)
            {
                errmsg = "找不到指定目录";
                return false;
            }
            if(_wikiToDirRepo.RemoveWikisFromDir(new() { wikiId }, dirId, out errmsg))
            {
                var w = _wikiRepo.CachedItemById(wikiId);
                _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.FileDir, dirId, wikiId,
                    $"从 {dir.Name} ({dir.Id}) 移除词条 {w?.Title} ({w?.UrlPathName})");
                return true;
            }
            return false;
        }
        public bool Delete(int id, out string? errmsg)
        {
            var w = _wikiRepo.GetById(id);
            if(w is null)
            {
                errmsg = "找不到指定词条";
                return false;
            }
            var s = _wikiRepo.TryRemove(w, out errmsg);
            if (s)
            {
                wikiRefService.ReferencedWikiPropChangeHandle(id, w.Title, w.UrlPathName);
                _opRecordRepo.Record(OpRecordOpType.Remove, OpRecordTargetType.WikiItem, id, 0,
                    $"{w.Title} ({w.UrlPathName})");
            }
            return s;
        }
        public WikiItem? GetInfo(string urlPathName, out string? errmsg)
        {
            var res = _wikiRepo.GetByUrlPathName(urlPathName).FirstOrDefault();
            if(res is null)
            {
                errmsg = "未找到指定路径名的词条";
                return null;
            }
            errmsg = null;
            return res;
        }
        public bool EditInfo(int id, string? title,string? urlPathName, out string? errmsg)
        {
            var target = _wikiRepo.GetById(id);
            if (target is null)
            {
                errmsg = "未找到指定路径名的词条";
                return false;
            }
            string? originalTitle = target.Title;
            string? originalUrlPathName = target.UrlPathName;
            bool changed = originalTitle != title || originalUrlPathName != urlPathName;

            string record = "";
            if (originalTitle != title)
                record += $"将 {originalTitle} 更名为 {title} ; ";
            if (originalUrlPathName != urlPathName)
                record += $"将路径名 {originalUrlPathName} 改为 {urlPathName}";

            target.Title = title;
            target.UrlPathName = urlPathName;
            if (changed)
            {
                if (_wikiRepo.TryUpdate(target, out errmsg))
                {
                    wikiRefService.ReferencedWikiPropChangeHandle(
                        id, originalTitle, originalUrlPathName, title, urlPathName);
                    if(record.Length>0)
                        _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem, id, 0, record);
                    latestWikiExchangeService.Push();
                    return true;
                }
                else
                    return false;
            }
            errmsg = null;
            return true;
        }

        public bool Transfer(int id, int uid, out string? errmsg)
        {
            var currentUid = _operatingUserIdProvider.Get();
            var w = _wikiRepo.GetById(id);
            if (w is null)
            {
                errmsg = "找不到目标词条";
                return false;
            }
            if (w.OwnerUserId != currentUid)
            {
                errmsg = "只有词条所有者能转让";
                return false;
            }
            var targetUser = _userRepo.GetById(uid);
            if (targetUser is null)
            {
                errmsg = "找不到指定用户";
                return false;
            }
            w.OwnerUserId = uid;
            if (_wikiRepo.TryUpdate(w, out errmsg))
            {
                //TODO:词条的上次更新时间与模型的更新时间是两码事，必须做区分
                var recordStr = $"将 {w.Title} ({w.UrlPathName}) 转让给 {targetUser.Name} ({targetUser.Id})";
                _opRecordRepo.Record(OpRecordOpType.EditImportant, OpRecordTargetType.WikiItem, id, uid, recordStr);
                return true;
            }
            return false;
        }
        public bool SetSealed(int id, bool @sealed, out string? errmsg)
        {
            var w = _wikiRepo.GetById(id);
            if (w is null)
            {
                errmsg = "找不到指定的词条";
                return false;
            }
            w.Sealed = @sealed;
            var s = _wikiRepo.TryUpdate(w, out errmsg);
            if (s)
            {
                wikiRefService.ReferencedWikiPropChangeHandle(id, w.Title, w.UrlPathName);
                string opStr = @sealed ? "隐藏" : "解除隐藏";
                opStr += $" {w.Title} ({w.UrlPathName})";
                _opRecordRepo.Record(OpRecordOpType.EditImportant, OpRecordTargetType.WikiItem ,id, 0, opStr);
            }
            return s;
        }

        private void SetWikiUpdated(int wikiId)
        {
            _wikiRepo.UpdateTimeAndLuAndWikiActive(wikiId, true);
            latestWikiExchangeService.Push();
        }

        public class WikiItemIndexItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Update { get; set; }
            public WikiItemIndexItem(WikiItem w)
            {
                Id = w.Id;
                Title = w.Title ?? "";
                Update = w.Updated.ToString("yy/MM/dd HH:mm");
            }
        }

        public class WikiInDirLocationView
        {
            public int WikiId { get; set; }
            public string? Title { get; set; }
            public List<WikiInDirLocationItem> Locations { get; set; } = [];
            public class WikiInDirLocationItem
            {
                public WikiInDirLocationItem(int id, List<string> nameChain, StringBuilder sb)
                {
                    Id = id;
                    sb.Clear();
                    nameChain.ForEach(x => {
                        sb.Append("/ ");
                        sb.Append(x);
                    });
                    NameChain = sb.ToString();
                }
                public int Id { get; set; }
                public string NameChain { get; set; }
            }
        } 
    }
}
