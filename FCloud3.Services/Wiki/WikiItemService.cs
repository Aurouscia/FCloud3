using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Files;
using FCloud3.Entities.Messages;
using FCloud3.Entities.Wiki;
using FCloud3.Repos;
using FCloud3.Repos.Files;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using FCloud3.Services.Etc.Metadata;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Wiki.Paragraph;
using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.Services.Wiki
{
    public class WikiItemService
    {
        private readonly DbTransactionService _transaction;
        private readonly WikiItemRepo _wikiRepo;
        private readonly WikiItemMetadataService _wikiMetadataService;
        private readonly WikiToDirRepo _wikiToDirRepo;
        private readonly WikiParaRepo _paraRepo;
        private readonly TextSectionRepo _textSectionRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly FileDirRepo _fileDirRepo;
        private readonly FreeTableRepo _freeTableRepo;
        private readonly CacheExpTokenService _cacheExpTokenService;
        private readonly OpRecordRepo _opRecordRepo;
        private readonly IOperatingUserIdProvider _operatingUserIdProvider;
        private readonly IStorage _storage;
        public const int maxWikiTitleLength = 30;
        public WikiItemService(
            DbTransactionService transaction,
            WikiItemRepo wikiRepo,
            WikiItemMetadataService wikiMetadataService,
            WikiToDirRepo wikiToDirRepo,
            WikiParaRepo paraRepo,
            TextSectionRepo textSectionRepo,
            FileItemRepo fileItemRepo,
            FileDirRepo fileDirRepo,
            FreeTableRepo freeTableRepo,
            CacheExpTokenService cacheExpTokenService,
            OpRecordRepo opRecordRepo,
            IOperatingUserIdProvider operatingUserIdProvider,
            IStorage storage)
        {
            _transaction = transaction;
            _wikiRepo = wikiRepo;
            _wikiMetadataService = wikiMetadataService;
            _wikiToDirRepo = wikiToDirRepo;
            _paraRepo = paraRepo;
            _textSectionRepo = textSectionRepo;
            _fileItemRepo = fileItemRepo;
            _fileDirRepo = fileDirRepo;
            _freeTableRepo = freeTableRepo;
            _cacheExpTokenService = cacheExpTokenService;
            _opRecordRepo = opRecordRepo;
            _operatingUserIdProvider = operatingUserIdProvider;
            _storage = storage;
        }
        public WikiItem? GetById(int id)
        {
            return _wikiRepo.GetById(id);
        }
        public IndexResult<WikiItemIndexItem> Index(IndexQuery query)
        {
            return _wikiRepo.IndexFilterOrder(query).TakePage(query,x=>new WikiItemIndexItem(x));
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

            List<WikiParaDisplay> paraObjs = paras.ConvertAll(x =>
            {
                WikiParaType type = x.Type;
                WikiParaDisplay? paraDisplay = null;
                if (type == WikiParaType.Text)
                {
                    var obj = textParaObjs.Find(p => p.Id == x.ObjectId);
                    if(obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Title, obj.ContentBrief, x.NameOverride, WikiParaType.Text, 0);
                }
                else if(type == WikiParaType.File)
                {
                    var obj = fileParaObjs.Find(p => p.Id == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.DisplayName, _storage.FullUrl(obj.StorePathName??"missing"), x.NameOverride, WikiParaType.File, obj.ByteCount);
                }
                else if(type == WikiParaType.Table)
                {
                    var obj = tableParaObjs.Find(p => p.Id == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Name, obj.Brief, x.NameOverride, WikiParaType.Table, 0);
                }
                paraDisplay ??= new WikiParaPlaceholder(type).ToDisplay(x);
                return paraDisplay;
            });
            return paraObjs;
        }
        public bool InsertPara(int wikiId, int afterOrder, WikiParaType type, out string? errmsg)
        {
            string? msg = null;
            bool success = _transaction.DoTransaction(() =>
            {
                var itsParas = GetWikiParas(wikiId);
                itsParas.EnsureOrderDense();
                var moveBackwards = itsParas.FindAll(x => x.Order > afterOrder);
                moveBackwards.ForEach(x => x.Order++);

                WikiPara p = new()
                {
                    WikiItemId = wikiId,
                    Order = afterOrder+1,
                    Type = type
                };
                if(!_paraRepo.TryAdd(p, out msg))
                    return false;

                if (!_paraRepo.TryEditRange(itsParas, out msg))
                    return false;
                success = true;
                return true;
            });
            errmsg = msg;
            if (success)
            {
                SetWikiUpdated(wikiId);
                var name = _wikiMetadataService.Get(wikiId)?.Title;
                _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem, 
                    $"为 {name} 插入了新 {WikiParaTypes.Readable(type)} 段落");
            }
            return success;
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
            }
            orderedParas.ResetOrder();
            if (!_paraRepo.TryEditRange(orderedParas, out errmsg))
                return false;

            SetWikiUpdated(wikiId);
            var name = _wikiMetadataService.Get(wikiId)?.Title;
            _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem,
                $"为 {name} 调整段落顺序");
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
            string? msg = null;
            using var t = _transaction.BeginTransaction();

            var editParasSuccess = !_paraRepo.TryEditRange(paras, out msg);
            var removeParaSuccess = false;
            if (editParasSuccess)
                removeParaSuccess = !_paraRepo.TryRemove(target, out msg);

            var success = removeParaSuccess && editParasSuccess;
            if (success)
                t.Commit();
            else
                t.Rollback();
            errmsg = msg;
            if (success)
            {
                SetWikiUpdated(id);
                var name = _wikiMetadataService.Get(id)?.Title;
                _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem,
                    $"从 {name} 移除了段落");
            }
            return success;
        }

        public bool CreateInDir(string title,string urlPathName,int dirId, out string? errmsg)
        {
            var newWiki = new WikiItem()
            {
                Title = title,
                UrlPathName = urlPathName,
            };
            int id = _wikiRepo.TryAddAndGetId(newWiki, out errmsg);
            if (id > 0)
            {
                if(_wikiToDirRepo.AddWikisToDir([id], dirId, out errmsg))
                {
                    int uid = _operatingUserIdProvider.Get();
                    _wikiMetadataService.Create(id, uid, title, urlPathName);
                    _opRecordRepo.Record(OpRecordOpType.Create, OpRecordTargetType.WikiItem, $" {title} ({urlPathName})");
                    return true;
                }
            }
            return false;
        }
        public bool RemoveFromDir(int wikiId, int dirId, out string? errmsg)
        {
            if(_wikiToDirRepo.RemoveWikisFromDir(new() { wikiId }, dirId, out errmsg))
            {
                var d = _fileDirRepo.GetqById(dirId).Select(x=>x.Name).FirstOrDefault();
                var w = _wikiMetadataService.Get(wikiId);
                if (w is not null && d is not null)
                    _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.FileDir, $"从 {d} 移除词条 {w.Title} ({w.UrlPathName})");
                return true;
            }
            return false;
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
            bool changed = target.Title != title || target.UrlPathName != urlPathName;

            string record = "";
            if (target.Title != title)
                record += $"将 {target.Title} 更名为 {title} ; ";
            if (target.UrlPathName != urlPathName)
                record += $"将路径名 {target.UrlPathName} 改为 {urlPathName}";

            target.Title = title;
            target.UrlPathName = urlPathName;
            if (changed)
            {
                if (_wikiRepo.TryEdit(target, out errmsg))
                {
                    _cacheExpTokenService.WikiItemNamePathInfo.CancelAll();
                    _wikiMetadataService.Update(id, w =>
                    {
                        w.Title = title;
                        w.UrlPathName = urlPathName;
                        w.Update = DateTime.Now;
                    });
                    if(record.Length>0)
                        _opRecordRepo.Record(OpRecordOpType.Edit, OpRecordTargetType.WikiItem, record);
                    return true;
                }
                else
                    return false;
            }
            errmsg = null;
            return true;
        }

        private void SetWikiUpdated(int wikiId)
        {
            _wikiRepo.SetUpdateTime(wikiId);
            _wikiMetadataService.Update(wikiId, w =>
            {
                w.Update = DateTime.Now;
            });
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
    }
}
