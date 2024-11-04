using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using System.Security.Cryptography;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Table;

namespace FCloud3.Services.Wiki
{
    public class WikiTitleContainService(
        WikiTitleContainRepo wikiTitleContainRepo,
        WikiItemRepo wikiItemRepo,
        WikiItemService wikiItemService,
        WikiParaRepo wikiParaRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo,
        DbTransactionService dbTransactionService)
    {
        private readonly WikiTitleContainRepo _wikiTitleContainRepo = wikiTitleContainRepo;
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiItemService _wikiItemService = wikiItemService;
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;
        private readonly DbTransactionService _dbTransactionService = dbTransactionService;

        public List<WikiTitleContain> GetByTypeAndObjId(WikiTitleContainType type, int objId)
        {
            return _wikiTitleContainRepo.GetByTypeAndObjId(type, objId);
        }
        
        public WikiTitleContainAutoFillResult AutoFill(int objId, WikiTitleContainType containType, string? content)
        {
            WikiTitleContainAutoFillResult res = new();
            if (content is null)
                return res;                
            //之前被删过的就不会再自动添加
            IQueryable<int> excludeWikiIds = _wikiTitleContainRepo
                .BlackListed
                .WithTypeAndId(containType, objId)
                .Select(x => x.WikiId);
            //自身的词条不添加
            IQueryable<int> containingSelf = _wikiParaRepo.WikiContainingIt(
                _wikiTitleContainRepo.ContainType2ParaType(containType), objId);

            var wikis = _wikiItemRepo.AllCachedItems()
                .Where(x => x.Title != null && content.Contains(x.Title))
                .Where(x => !excludeWikiIds.Contains(x.Id))
                .Where(x => !containingSelf.Contains(x.Id))
                .ToList();
            wikis.ForEach(x =>
            {
                res.Add(x.Id, x.Title!);
            });
            return res;
        }
        public WikiTitleContainAutoFillResult AutoFill(int objId, WikiTitleContainType containType)
        {
            string? content;
            if (containType == WikiTitleContainType.TextSection)
                content = _textSectionRepo.GetqById(objId).Select(x => x.Content).FirstOrDefault();
            else
                content = _freeTableRepo.GetqById(objId).Select(x => x.Data).FirstOrDefault();
            return AutoFill(objId, containType, content);
        }
        public WikiTitleContainListModel GetAll(WikiTitleContainType type, int objectId)
        {
            var list = _wikiTitleContainRepo.GetByTypeAndObjId(type, objectId, true);
            var wikiIds = list.ConvertAll(x => x.WikiId);
            var wikis = _wikiItemRepo.CachedItemsByIds(wikiIds);
            WikiTitleContainListModel model = new();
            list.ForEach(c =>
            {
                string? title = wikis.Where(x => x.Id == c.WikiId).Select(x=>x.Title).FirstOrDefault();
                if(title is not null)
                    model.Add(c.Id, c.WikiId, title);
            });
            return model;
        }

        public WikiTitleContainForWikiResult GetAllForWiki(int wikiId)
        {
            var textType = _wikiTitleContainRepo.ContainType2ParaType(WikiTitleContainType.TextSection);
            var tableType = _wikiTitleContainRepo.ContainType2ParaType(WikiTitleContainType.FreeTable);
            var paras = _wikiItemService.GetWikiParaDisplays(wikiId);
            var allTextIds = paras
                .FindAll(x => x.Type == textType)
                .ConvertAll(x=>x.UnderlyingId);
            var allTableIds = paras
                .FindAll(x => x.Type == tableType)
                .ConvertAll(x=>x.UnderlyingId);
            var contains = _wikiTitleContainRepo
                .GetByTypeAndObjIds(WikiTitleContainType.TextSection, allTextIds);
            contains.AddRange(_wikiTitleContainRepo
                .GetByTypeAndObjIds(WikiTitleContainType.FreeTable, allTableIds));
            var relatedWikiIds = contains.ConvertAll(x => x.WikiId).Distinct().ToList();
            var relatedWikis = _wikiItemRepo.CachedItemsByIds(relatedWikiIds);
            WikiTitleContainForWikiResult res = new();
            paras.ForEach(p =>
            {
                var itsList = res.Add(p.ParaId, p.UnderlyingId, p.NameOverride ?? p.Title ?? "未命名段落").Items;
                var itsConts = contains
                    .FindAll(x => x.ObjectId == p.UnderlyingId 
                                  && x.Type == _wikiTitleContainRepo.ParaType2ContainType(p.Type));
                itsConts.ForEach(c =>
                {
                    var title = relatedWikis.Find(x => x.Id == c.WikiId)?.Title;
                    if(title is {})
                        itsList.Add(new()
                        {
                            Id = c.Id,
                            WikiId = c.WikiId,
                            WikiTitle = title
                        });
                });
            });
            return res;
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
                WikiParaType pt = _wikiTitleContainRepo.ContainType2ParaType(type);
                var wIds = _wikiParaRepo.WikiContainingIt(pt, objectId).ToList();
                _wikiItemRepo.UpdateTimeAndLu(wIds);
            }
            _dbTransactionService.CommitTransaction(t);
            return true;
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
        }
        public class WikiTitleContainListModelItem
        {
            public int Id { get; set; }
            public string? WikiTitle { get; set; }
            public int WikiId { get; set; }
        }

        public class WikiTitleContainForWikiResult
        {
            public List<WikiTitleContainForWikiResultItem> Items { get; set; } = [];
            public WikiTitleContainForWikiResultItem Add(int paraId, int underlyingId, string paraDisplayName)
            {
                var added = new WikiTitleContainForWikiResultItem(paraId, underlyingId, paraDisplayName);
                Items.Add(added);
                return added;
            }
            public class WikiTitleContainForWikiResultItem(int pid, int objId, string pname)
            {
                public int PId { get; set; } = pid;
                public int ObjId { get; set; } = objId;
                public string PDName { get; set; } = pname;
                public List<WikiTitleContainListModelItem> Items { get; set; } = [];
            }
        }

        public class WikiTitleContainAutoFillResult
        {
            public List<WikiTitleContainAutoFillResultItem> Items { get; } = [];
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
