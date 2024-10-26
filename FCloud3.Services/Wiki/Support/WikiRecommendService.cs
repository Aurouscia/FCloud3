using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Wiki.Support
{
    public class WikiRecommendService(
        WikiToDirRepo wikiToDirRepo,
        WikiItemRepo wikiItemRepo,
        FileDirRepo fileDirRepo)
    {
        private readonly Random _random = new();
        public WikiRecommendModel Get(string pathName)
        {
            var res = new WikiRecommendModel();
            var dirs = (
                from d in fileDirRepo.Existing
                from w in wikiItemRepo.Existing
                from wd in wikiToDirRepo.Existing
                where w.UrlPathName == pathName
                where wd.WikiId == w.Id
                where d.Id == wd.DirId
                select new WikiRecommendModel.Dir(d.Id, d.Name)).ToList();
            res.Dirs.AddRange(RandomSelect(dirs, 4));
            var dirIds = res.Dirs.ConvertAll(x => x.Id);
            var neighborIds = wikiToDirRepo.GetWikiIdsByDirs(dirIds);

            var thisId = wikiItemRepo.CachedItemByPred(x=>x.UrlPathName == pathName)?.Id ?? 0;
            neighborIds.Remove(thisId);
            var neighbors = wikiItemRepo.CachedItemsByIds(neighborIds)
                .ConvertAll(x=>new WikiRecommendModel.Wiki(x.Title, x.UrlPathName));
            res.Wikis.AddRange(RandomSelect(neighbors, 8));
            return res;
        }

        private List<T> RandomSelect<T>(List<T> values, int count = 3)
        {
            var gened = new List<T>(count);
            while (gened.Count < count && values.Count > 0)
            {
                var r = _random.Next(values.Count);
                var selected = values[r];
                values.RemoveAt(r);
                gened.Add(selected);
            }
            return gened;
        }
        public class WikiRecommendModel
        {
            public List<Dir> Dirs { get; set; } = [];
            public List<Wiki> Wikis { get; set; } = [];
            public class Dir(int id, string? name)
            {
                public int Id { get; set; } = id;
                public string? Name { get; set; } = name;
            }
            public class Wiki(string? title, string? urlPathName)
            {
                public string? Title { get; set; } = title;
                public string? UrlPathName { get; set; } = urlPathName;
            }
        }
    }
}
