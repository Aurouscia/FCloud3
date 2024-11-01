using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.Services.Etc
{
    public class QuickSearchService(
        WikiItemRepo wikiItemRepo,
        UserRepo userRepo,
        UserGroupRepo userGroupRepo,
        FileItemRepo fileItemRepo,
        MaterialRepo materialRepo,
        FileDirRepo fileDirRepo,
        WikiToDirRepo wikiToDirRepo,
        IOperatingUserIdProvider userIdProvider,
        IStorage storage)
    {
        private const int maxCount = 8;

        public QuickSearchResult SearchWikiItem(string str, int excludeDir, bool isAdmin)
        {
            var uid = userIdProvider.Get();
            var q = wikiItemRepo.QuickSearch(str, isAdmin, uid);
            if(excludeDir > 0)
            {
                var excludeWikis = wikiToDirRepo.Existing
                    .Where(x => x.DirId == excludeDir)
                    .Select(x => x.WikiId);
                q = q.Where(w => !excludeWikis.Contains(w.Id));
            }
            var items = q.Select(x => new { x.Title, x.UrlPathName, x.Id }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Title ?? "N/A", x.UrlPathName, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchUser(string str)
        {
            var q = userRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Name, x.Id }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Name ?? "N/A", null, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchUserGroup(string str)
        {
            var q = userGroupRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Name, x.Id }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Name ?? "N/A", null, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchFileItem(string str)
        {
            var q = fileItemRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Id, x.DisplayName, x.StorePathName }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.DisplayName ?? "N/A", x.StorePathName, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchMaterial(string str)
        {
            var q = materialRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Id, x.Name, x.StorePathName }).Take(maxCount).ToList();
            QuickSearchResult res = new(true);
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Name ?? "N/A", storage.FullUrl(x.StorePathName ?? "??"), x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchFileDir(string str)
        {
            var dirs = fileDirRepo.QuickSearch(str).Select(x => new { x.Id, x.Name }).Take(maxCount).ToList();
            var nameChains = fileDirRepo.GetNameChainsByIds(dirs.ConvertAll(x => x.Id));
            QuickSearchResult res = new();
            nameChains.ForEach(x =>
            {
                var last = x.nameChain.LastOrDefault();
                if (last is null)
                    return;
                var exceptLast = x.nameChain[..^1];
                var path = string.Join('/', exceptLast);
                res.Items.Add(new(last, path, x.id));
            });
            return res;
        }
    }

    public class QuickSearchResult
    {
        public List<QuickSearchResultItem> Items { get; set; }
        public bool DescIsSrc { get; set; }
        public QuickSearchResult(bool descIsSrc = false)
        {
            Items = new();
            DescIsSrc = descIsSrc;
        }
        public class QuickSearchResultItem
        {
            public string Name { get; set; }
            public string? Desc { get; set; }
            public int Id { get; set; }
            public QuickSearchResultItem(string name, string? desc, int id)
            {
                Name = name;
                Desc = desc;
                Id = id;
            }
        }
    }
}
