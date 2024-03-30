using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Wiki
{
    public class WikiTitleContainService
    {
        private readonly WikiTitleContainRepo _wikiTitleContainRepo;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly DbTransactionService _dbTransactionService;

        public WikiTitleContainService(
            WikiTitleContainRepo wikiTitleContainRepo,
            WikiItemRepo wikiItemRepo,
            DbTransactionService dbTransactionService)
        {
            _wikiTitleContainRepo = wikiTitleContainRepo;
            _wikiItemRepo = wikiItemRepo;
            _dbTransactionService = dbTransactionService;
        }

        public List<WikiTitleContain> GetByTypeAndObjId(WikiTitleContainType type, int objId)
        {
            return _wikiTitleContainRepo.GetByTypeAndObjId(type, objId);
        }

        public WikiTitleContainAutoFillResult AutoFill(string content)
        {
            var wikis = _wikiItemRepo.Existing
                .Where(x => x.Title != null && content.Contains(x.Title))
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
            var list = _wikiTitleContainRepo.GetByTypeAndObjId(type, objectId);
            var wikiIds = list.ConvertAll(x => x.WikiId);
            var wikis = _wikiItemRepo.GetRangeByIds(wikiIds).Select(x => new { x.Id,x.Title }).ToList();
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
            var existing = _wikiTitleContainRepo.GetByTypeAndObjId(type, objectId);
            var needRemove = existing.FindAll(x => !wikiIds.Contains(x.WikiId));
            var needAdd = wikiIds.FindAll(x => !existing.Any(c => c.WikiId == x));
            _dbTransactionService.BeginTransaction();
            _wikiTitleContainRepo.TryRemoveRangePermanent(needRemove, out errmsg);
            if(errmsg is not null)
            {
                _dbTransactionService.RollbackTransaction();
                return false;
            }
            var newObjs = needAdd.ConvertAll(x => new WikiTitleContain
            {
                WikiId = x,
                Type = type,
                ObjectId = objectId,
            });
            _wikiTitleContainRepo.TryAddRange(newObjs, out errmsg);
            if(errmsg is not null)
            {
                _dbTransactionService.RollbackTransaction();
                return false;
            }
            else
            {
                _dbTransactionService.CommitTransaction();
                return true;
            }
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
