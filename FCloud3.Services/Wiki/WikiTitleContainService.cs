using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Table;

namespace FCloud3.Services.Wiki
{
    public class WikiTitleContainService(
        WikiTitleContainRepo wikiTitleContainRepo,
        WikiItemRepo wikiItemRepo,
        WikiParaRepo wikiParaRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo)
    {
        private readonly WikiTitleContainRepo _wikiTitleContainRepo = wikiTitleContainRepo;
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;

        [Obsolete]
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
        [Obsolete]
        public WikiTitleContainAutoFillResult AutoFill(int objId, WikiTitleContainType containType)
        {
            string? content;
            if (containType == WikiTitleContainType.TextSection)
                content = _textSectionRepo.GetqById(objId).Select(x => x.Content).FirstOrDefault();
            else
                content = _freeTableRepo.GetqById(objId).Select(x => x.Data).FirstOrDefault();
            return AutoFill(objId, containType, content);
        }
        public WikiTitleContainListModel GetContains(WikiTitleContainType type, int objectId)
        {
            var list = _wikiTitleContainRepo.CachedContains(type, objectId, true).ToList();
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

        public WikiTitleContainForWikiResult GetContainsForWiki(int wikiId)
        {
            throw new NotImplementedException();
        }
        public void SetContains(WikiTitleContainType type, int objectId, List<int> wikiIds)
        {
            var changed = _wikiTitleContainRepo.SetStatus(type, objectId, wikiIds);
            if (changed > 0)
            {
                WikiParaType pt = _wikiTitleContainRepo.ContainType2ParaType(type);
                var wIds = _wikiParaRepo.WikiContainingIt(pt, objectId).ToList();
                _wikiItemRepo.UpdateTimeAndLu(wIds);
            }
        }
        public void AutoAppendForGroups(
            List<(WikiTitleContainType containType, int objId, List<int> excludeWIds, string content)> groups)
        {
            WikiTitleContainAutoFillResult res = new();
            if (groups.Count == 0)
                return;
            List<(WikiTitleContainType containType, int objId, List<int> wikiIds)> wIdss = [];
            foreach (var group in groups) {
                var wIds = _wikiItemRepo.AllCachedItems()
                    .Where(x => x.Title != null && group.content.Contains(x.Title))
                    .Select(x => x.Id)
                    .Except(group.excludeWIds)
                    .ToList();
                wIdss.Add((group.containType, group.objId, wIds));
            }
            _wikiTitleContainRepo.AppendForGroups(wIdss);
        }
        public void AutoAppendForOne(WikiTitleContainType containType, int objId, List<int> excludeWIds, string content)
        {
            AutoAppendForGroups([(containType, objId, excludeWIds, content)]);
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
