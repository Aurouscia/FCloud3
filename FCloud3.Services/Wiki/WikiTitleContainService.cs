using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using FCloud3.Repos.Etc.Caching;

namespace FCloud3.Services.Wiki
{
    public class WikiTitleContainService(
        WikiTitleContainRepo wikiTitleContainRepo,
        WikiItemRepo wikiItemRepo,
        WikiParaRepo wikiParaRepo,
        WikiItemCaching wikiItemCaching,
        DbTransactionService dbTransactionService,
        CacheExpTokenService cacheExpTokenService)
    {
        private readonly WikiTitleContainRepo _wikiTitleContainRepo = wikiTitleContainRepo;
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly WikiItemCaching _wikiItemCaching = wikiItemCaching;
        private readonly DbTransactionService _dbTransactionService = dbTransactionService;
        private readonly CacheExpTokenService _cacheExpTokenService = cacheExpTokenService;

        public List<WikiTitleContain> GetByTypeAndObjId(WikiTitleContainType type, int objId)
        {
            return _wikiTitleContainRepo.GetByTypeAndObjId(type, objId);
        }

        /// <summary>
        /// 用于排除emoji，不知道为什么，任意字符串.Contains("🐷")都会返回true<br/>
        /// 搞一个生僻字符串排除emoji即可
        /// </summary>
        private const string someStrangeWord = "亐髵";
        public WikiTitleContainAutoFillResult AutoFill(int objId, WikiTitleContainType containType, string content)
        {
            //之前被删过的就不会再自动添加
            IQueryable<int> excludeWikiIds = _wikiTitleContainRepo
                .BlackListed
                .WithTypeAndId(containType, objId)
                .Select(x => x.WikiId);
            //自身的词条不添加
            IQueryable<int> containingSelf = _wikiParaRepo.WikiContainingIt(GetWikiParaType(containType), objId);

            var wikis = _wikiItemRepo.Existing
                .Where(x => x.Title != null && content.Contains(x.Title) && !someStrangeWord.Contains(x.Title))
                .Where(x => !excludeWikiIds.Contains(x.Id))
                .Where(x => !containingSelf.Contains(x.Id))
                .Select(x => new { x.Id, x.Title }).ToList();
            WikiTitleContainAutoFillResult res = new();
            wikis.ForEach(x =>
            {
                res.Add(x.Id, x.Title!);
            });
            return res;
        }
        public WikiTitleContainListModel GetAll(WikiTitleContainType type, int objectId)
        {
            var list = _wikiTitleContainRepo.GetByTypeAndObjId(type, objectId, true);
            var wikiIds = list.ConvertAll(x => x.WikiId);
            var wikis = _wikiItemCaching.GetRange(wikiIds);
            WikiTitleContainListModel model = new();
            list.ForEach(c =>
            {
                string? title = wikis.Where(x => x.Id == c.WikiId).Select(x=>x.Title).FirstOrDefault();
                if(title is not null)
                    model.Add(c.Id, c.WikiId, title);
            });
            return model;
        }
        public bool SetAll(WikiTitleContainType type, int objectId, List<int> wikiIds, out string? errmsg)
        {
            wikiIds = wikiIds.Distinct().ToList();
            var all = _wikiTitleContainRepo.GetByTypeAndObjId(type, objectId, false);
            var needRemove = all.FindAll(x => !x.BlackListed && !wikiIds.Contains(x.WikiId));
            var needRecover = all.FindAll(x => x.BlackListed && wikiIds.Contains(x.WikiId));
            var needAdd = wikiIds.FindAll(x => !all.Any(c => c.WikiId == x));
            var newObjs = needAdd.ConvertAll(x => new WikiTitleContain
            {
                WikiId = x,
                Type = type,
                ObjectId = objectId,
            });

            using var t = _dbTransactionService.BeginTransaction();
            if (!_wikiTitleContainRepo.SetStatus(needRemove, needRecover, newObjs, out errmsg))
            {
                _dbTransactionService.RollbackTransaction(t);
                return false;
            }

            if (newObjs.Count > 0 || needRemove.Count > 0 || needRecover.Count > 0)
            {
                WikiParaType pt = GetWikiParaType(type);
                var wIds = _wikiParaRepo.WikiContainingIt(pt, objectId).ToList();
                foreach(int w in wIds)
                    _cacheExpTokenService.WikiTitleContain.GetByKey(w).CancelAll();
                _wikiItemRepo.UpdateTime(wIds);
            }
            _dbTransactionService.CommitTransaction(t);
            return true;
        }

        private static WikiParaType GetWikiParaType(WikiTitleContainType wikiTitleContainType)
        {
            if (wikiTitleContainType == WikiTitleContainType.TextSection)
                return WikiParaType.Text;
            else if (wikiTitleContainType == WikiTitleContainType.FreeTable)
                return WikiParaType.Table;
            throw new NotImplementedException();
        }

        public class WikiTitleContainListModel
        {
            public List<WikiTitleContainListModelItem> Items { get; }
            public WikiTitleContainListModel()
            {
                Items = new();
            }
            public void Add(int id, int wikiId, string wikiTitle)
            {
                Items.Add(new WikiTitleContainListModelItem
                {
                    Id = id,
                    WikiId = wikiId,
                    WikiTitle = wikiTitle
                });
            }
            public class WikiTitleContainListModelItem
            {
                public int Id { get; set; }
                public string? WikiTitle { get; set; }
                public int WikiId { get; set; }
            }
        }

        public class WikiTitleContainAutoFillResult
        {
            public List<WikiTitleContainAutoFillResultItem> Items { get; }
            public WikiTitleContainAutoFillResult()
            {
                Items = new();
            }
            public void Add(int id,string title)
            {
                Items.Add(new() { Id = id, Title = title });
            }
            public class WikiTitleContainAutoFillResultItem
            {
                public int Id { get; set; }
                public string? Title { get; set; }
            }
        }
    }
}
