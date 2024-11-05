using FCloud3.Entities.Identities;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Identities;
using Newtonsoft.Json;

namespace FCloud3.Services.Etc
{
    public class WikiCenteredHomePageService(
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        FileDirRepo fileDirRepo,
        UserRepo userRepo,
        MaterialRepo materialRepo,
        LatestWikiExchangeService latestWikiExchangeService,
        IOperatingUserIdProvider userIdProvider,
        IStorage storage)
    {
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiToDirRepo _wikiToDirRepo = wikiToDirRepo;
        private readonly FileDirRepo _fileDirRepo = fileDirRepo;
        private readonly UserRepo _userRepo = userRepo;
        private readonly MaterialRepo _materialRepo = materialRepo;
        private readonly IOperatingUserIdProvider _userIdProvider = userIdProvider;
        private readonly IStorage _storage = storage;

        public WikiCenteredHomePage Get()
        {
            int latestCount = 8;
            int nearCount = 30;
            int randRange = 300;

            var tops = (
                from w in _wikiItemRepo.ExistingAndNotSealedAndEdited
                from dd in _fileDirRepo.Existing
                from d in _fileDirRepo.Existing
                from wd in _wikiToDirRepo.Existing
                where wd.WikiId == w.Id && wd.DirId == dd.Id
                where d.Id == dd.RootDir
                group w by d into wdg
                orderby wdg.Key.Updated descending
                select new {
                    Wiki = wdg
                        .OrderByDescending(x => x.LastActive)
                        .Select(x => new { x.UrlPathName, x.Title })
                        .FirstOrDefault(),
                    FileDir = new
                    {
                        UrlPathName = wdg.Key.UrlPathName,
                        Name = wdg.Key.Name
                    },
                })
                .Take(nearCount).ToList();

            var topPairs = tops.ConvertAll(x => new WikiCenteredHomePage.Pair(
                x.Wiki.UrlPathName,
                x.Wiki.Title,
                x.FileDir.UrlPathName,
                x.FileDir.Name));

            var latestWikisRaw = _wikiItemRepo.ExistingAndNotSealedAndEdited
                .OrderByDescending(x => x.LastActive)
                .Take(latestCount).ToList()
                .ConvertAll(x => new
                {
                    OwnerId = x.OwnerUserId,
                    UrlPathName = x.UrlPathName ?? "??",
                    Title = x.Title ?? "??",
                    Active = x.LastActive
                });

            var randFromWikiIds = _wikiItemRepo.ExistingAndNotSealed
                .OrderByDescending(x => x.LastActive)
                .Select(x => x.Id)
                .Take(randRange).ToList();
            var randedWikisIds = RandomPick(randFromWikiIds, latestWikisRaw.Count);
            var randomWikisRaw = _wikiItemRepo.GetRangeByIdsOrdered<WikiCenteredHomePage.WikiWithOwner>(randedWikisIds, x
                => x.Select(w => new { w.Id, w.UrlPathName, w.Title, w.OwnerUserId, w.LastActive })
                    .ToDictionary(
                        w => w.Id,
                        w => new WikiCenteredHomePage.WikiWithOwner(
                            w.UrlPathName ?? "??", w.Title ?? "??", w.OwnerUserId, w.LastActive)));

            int uid = _userIdProvider.Get();
            if (uid > 0)
            {
                //如果当前是登录用户，那么找出包含其拥有词条最多的顶级文件夹，并插入到第一个位置
                var containingMine = (
                    from w in _wikiItemRepo.ExistingAndNotSealed.Where(x => x.OwnerUserId == uid)
                    from dd in _fileDirRepo.Existing
                    from d in _fileDirRepo.Existing
                    from wd in _wikiToDirRepo.Existing
                    where wd.WikiId == w.Id && wd.DirId == dd.Id
                    where d.Id == dd.RootDir
                    group w by d
                    into wdg
                    orderby wdg.Count() descending
                    select new
                    {
                        Wiki = wdg
                            .OrderByDescending(x => x.LastActive)
                            .Select(x => new { x.UrlPathName, x.Title })
                            .FirstOrDefault(),
                        FileDir = new
                        {
                            UrlPathName = wdg.Key.UrlPathName,
                            Name = wdg.Key.Name
                        }
                    }).Take(1).ToList();
                var containingMinePairs = containingMine.ConvertAll(x => new WikiCenteredHomePage.Pair(
                    x.Wiki.UrlPathName,
                    x.Wiki.Title,
                    x.FileDir.UrlPathName,
                    x.FileDir.Name));
                topPairs.RemoveAll(x => containingMinePairs.Any(c => c.DPath == x.DPath));
                topPairs.InsertRange(0, containingMinePairs);
            }

            var latestOwners = latestWikisRaw.ConvertAll(x => x.OwnerId);
            var randomOwners = randomWikisRaw.ConvertAll(x => x.Owner);
            var owners = latestOwners.Union(randomOwners);
            var avtInfo = (
                from u in _userRepo.Existing
                from m in _materialRepo.Existing
                where owners.Contains(u.Id)
                where u.AvatarMaterialId == m.Id
                select new { Uid = u.Id, m.StorePathName })
                .ToList();
            List<WikiCenteredHomePage.WikiWithAvt> latestWikis = latestWikisRaw.ConvertAll(x =>
            {
                var avt = avtInfo.Find(y => y.Uid == x.OwnerId)?.StorePathName;
                string avtUrl = avt is { } ? _storage.FullUrl(avt) : User.defaultAvatar;
                return new WikiCenteredHomePage.WikiWithAvt(x.UrlPathName, x.Title, avtUrl, x.Active);
            });
            List<WikiCenteredHomePage.WikiWithAvt> randomWikis = randomWikisRaw.ConvertAll(x =>
            {
                var avt = avtInfo.Find(y => y.Uid == x.Owner)?.StorePathName;
                string avtUrl = avt is { } ? _storage.FullUrl(avt) : User.defaultAvatar;
                return new WikiCenteredHomePage.WikiWithAvt(x.Path, x.Title, avtUrl, x.Active);
            });
            foreach (var pulled in latestWikiExchangeService.GetItems())
            {
                if(pulled.Url is { } && pulled.Avt is { } && pulled.Text is { })
                {
                    latestWikis.Add(new WikiCenteredHomePage.WikiWithAvt(
                        pulled.Url, pulled.Text, pulled.Avt, pulled.Time));
                }
            }
            if(latestWikis.Count > latestCount)
            {
                latestWikis = latestWikis
                    .OrderByDescending(x => x.Active)
                    .Take(latestCount).ToList();
            }
            
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
            public WikiCenteredHomePage(List<WikiWithAvt> latestWikis, List<WikiWithAvt> randomWikis, List<Pair> topDirs)
            {
                LatestWikis = latestWikis;
                RandomWikis = randomWikis;
                TopDirs = topDirs;
            }
            public List<WikiWithAvt> LatestWikis { get; set; }
            public List<WikiWithAvt> RandomWikis { get; set; }
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
            public class Wiki(string urlPathName, string title, DateTime active)
            {
                public string Path { get; set; } = urlPathName;
                public string Title { get; set; } = title;
                [JsonIgnore]
                public DateTime Active { get; set; } = active;
            }
            public class WikiWithAvt(string urlPathName, string title, string avt, DateTime active) 
                :Wiki(urlPathName, title, active)
            {
                public string Avt { get; set; } = avt;
            }
            public class WikiWithOwner(string urlPathName, string title, int owner, DateTime active) 
                :Wiki(urlPathName, title, active)
            {
                public int Owner { get; set; } = owner;
            }
            public struct FileDir(string urlPathName, string name)
            {
                public string Path { get; set; } = urlPathName;
                public string Name { get; set; } = name;
            }
        }
    }
}