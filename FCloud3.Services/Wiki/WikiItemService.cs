using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Files;
using FCloud3.Entities.Wiki;
using FCloud3.Repos;
using FCloud3.Repos.Files;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Wiki.Paragraph;
using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.Services.Wiki
{
    public class WikiItemService
    {
        private readonly DbTransactionService _transaction;
        private readonly WikiItemRepo _wikiRepo;
        private readonly WikiToDirRepo _wikiToDirRepo;
        private readonly WikiParaRepo _paraRepo;
        private readonly TextSectionRepo _textSectionRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly FreeTableRepo _freeTableRepo;
        private readonly CacheExpTokenService _cacheExpTokenService;
        private readonly IStorage _storage;
        private readonly IMemoryCache _cache;
        public const int maxWikiTitleLength = 30;
        public WikiItemService(
            DbTransactionService transaction,
            WikiItemRepo wikiRepo,
            WikiToDirRepo wikiToDirRepo,
            WikiParaRepo paraRepo,
            TextSectionRepo textSectionRepo,
            FileItemRepo fileItemRepo,
            FreeTableRepo freeTableRepo,
            CacheExpTokenService cacheExpTokenService,
            IStorage storage,
            IMemoryCache cache)
        {
            _transaction = transaction;
            _wikiRepo = wikiRepo;
            _wikiToDirRepo = wikiToDirRepo;
            _paraRepo = paraRepo;
            _textSectionRepo = textSectionRepo;
            _fileItemRepo = fileItemRepo;
            _freeTableRepo = freeTableRepo;
            _cacheExpTokenService = cacheExpTokenService;
            _storage = storage;
            _cache = cache;
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
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Title, obj.ContentBrief, WikiParaType.Text, 0);
                }
                else if(type == WikiParaType.File)
                {
                    var obj = fileParaObjs.Find(p => p.Id == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.DisplayName, _storage.FullUrl(obj.StorePathName??"missing"), WikiParaType.File, obj.ByteCount);
                }
                else if(type == WikiParaType.Table)
                {
                    var obj = tableParaObjs.Find(p => p.Id == x.ObjectId);
                    if (obj is not null)
                        paraDisplay = new WikiParaDisplay(x, obj.Id, obj.Name, obj.Brief, WikiParaType.Table, 0);
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
            bool success = _transaction.DoTransaction(() =>
            {
                if (!_paraRepo.TryEditRange(paras, out msg))
                    return false;
                if (!_paraRepo.TryRemove(target, out msg))
                    return false;
                return true;
            });
            errmsg = msg;
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
                return _wikiToDirRepo.AddWikisToDir(new() { id }, dirId, out errmsg);
            }
            _cacheExpTokenService.WikiItemInfo.CancelAll();
            return false;
        }
        public bool RemoveFromDir(int wikiId, int dirId, out string? errmsg)
        {
            return _wikiToDirRepo.RemoveWikisFromDir(new() { wikiId}, dirId, out errmsg);
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
            target.Title = title;
            target.UrlPathName = urlPathName;
            if (changed)
                _cacheExpTokenService.WikiItemInfo.CancelAll();
            return _wikiRepo.TryEdit(target, out errmsg);
        }


        private const string allWikiItemsMetaCacheKey = "AllWikiItemsMeta";
        public List<WikiItemMetaData> WikiItemsMetaAll()
        {
            var res = _cache.Get<List<WikiItemMetaData>>(allWikiItemsMetaCacheKey);
            if (res is null)
            {
                var list = _wikiRepo.Existing.Select(x => new WikiItemMetaData(x.Id, x.Title, x.UrlPathName, x.Updated)).ToList();
                var cacheOptions = new MemoryCacheEntryOptions();
                cacheOptions.AddExpirationToken(_cacheExpTokenService.WikiItemInfo.GetCancelChangeToken());
                _cache.Set(allWikiItemsMetaCacheKey, list, cacheOptions);
                res = list;
            }
            return res;
        }
        public WikiItemMetaData? WikiItemsMeta(int id)
        {
            var all = WikiItemsMetaAll();
            return all.Find(x => x.Id == id);
        }
        public WikiItemMetaData? WikiItemsMeta(string pathName)
        {
            var all = WikiItemsMetaAll();
            return all.Find(x => x.UrlPathName == pathName);
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

        public class WikiItemMetaData
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? UrlPathName { get; set; }
            public DateTime Update { get; set; }
            public WikiItemMetaData(int id, string? title, string? urlPathName, DateTime update)
            {
                Id = id;
                Title = title;
                UrlPathName = urlPathName;
                Update = update;
            }
        }
    }
}
