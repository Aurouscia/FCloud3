using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Etc
{
    public class WikiCenteredHomePageService(
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        FileDirRepo fileDirRepo)
    {
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiToDirRepo _wikiToDirRepo = wikiToDirRepo;
        private readonly FileDirRepo _fileDirRepo = fileDirRepo;


        public WikiCenteredHomePage Get()
        {
            int latestCount = 8;
            int nearCount = 40;
            int randRange = 300;
            
            var tops = (
                from w in _wikiItemRepo.Existing
                from dd in _fileDirRepo.Existing
                from d in _fileDirRepo.Existing
                from wd in _wikiToDirRepo.Existing
                where wd.WikiId == w.Id && wd.DirId == dd.Id
                where d.Id == dd.RootDir
                group w by d into wdg
                orderby wdg.Key.Updated descending
                select new {
                    Wiki = wdg
                        .OrderByDescending(x=>x.Updated)
                        .Select(x=>new{x.UrlPathName, x.Title})
                        .FirstOrDefault(),
                    FileDir = new
                    {
                        UrlPathName = wdg.Key.UrlPathName,
                        Name = wdg.Key.Name
                    }
                })
                .Take(nearCount).ToList();
            var topPairs = tops.ConvertAll(x => new WikiCenteredHomePage.Pair(
                x.Wiki.UrlPathName,
                x.Wiki.Title, 
                x.FileDir.UrlPathName,
                x.FileDir.Name));
            
            var latestWikis = topPairs
                .Select(x => new WikiCenteredHomePage.Wiki(x.WPath, x.WTitle))
                .DistinctBy(x => x.Path).Take(latestCount).ToList();
            
            var randFromWikiIds = _wikiItemRepo.Existing
                .OrderByDescending(x => x.Updated)
                .Select(x => x.Id)
                .Take(randRange).ToList();
            var randedWikisIds = RandomPick(randFromWikiIds, latestWikis.Count);
            var randomWikis = _wikiItemRepo.GetRangeByIdsOrdered<WikiCenteredHomePage.Wiki>(randedWikisIds, x
                => x.Select(w => new { w.Id, w.UrlPathName, w.Title })
                    .ToDictionary(
                        w => w.Id,
                        w => new WikiCenteredHomePage.Wiki(w.UrlPathName, w.Title)));
            
            var model = new WikiCenteredHomePage(latestWikis, randomWikis, topPairs);
            return model;
        }

        
        private static List<int> RandomPick(List<int> fromIds, int maxCount)
        {
            var r = new Random();
            if (fromIds.Count <= maxCount)
                return fromIds;
            var res = new List<int>();
            while (res.Count < maxCount)
            {
                var idx = r.Next(0, fromIds.Count);
                var selected = fromIds[idx];
                fromIds.RemoveAt(idx);
                res.Add(selected);
            }
            return res;
        }
        
        public class WikiCenteredHomePage
        {
            public WikiCenteredHomePage(List<Wiki> latestWikis, List<Wiki> randomWikis, List<Pair> topDirs)
            {
                LatestWikis = latestWikis;
                RandomWikis = randomWikis;
                TopDirs = topDirs;
            }
            public List<Wiki> LatestWikis { get; set; }
            public List<Wiki> RandomWikis { get; set; }
            public List<Pair> TopDirs { get; set; }
            public struct Pair(
                string wikiUrlPathName, string wikiTitle,
                string fileDirUrlPathName, string fileDirName)
            {
                public string WPath { get; set; } = wikiUrlPathName;
                public string WTitle { get; set; } = wikiTitle;
                public string DPath { get; set; } = fileDirUrlPathName;
                public string DName { get; set; } = fileDirName;
            }  
            public struct Wiki(string urlPathName, string title)
            {
                public string Path { get; set; } = urlPathName;
                public string Title { get; set; } = title;
            }
            public struct FileDir(string urlPathName, string name)
            {
                public string Path { get; set; } = urlPathName;
                public string Name { get; set; } = name;
            }
        }
    }
}