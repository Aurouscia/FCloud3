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
        IStorage storage)
    {
        private const int maxCount = 8;
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly UserRepo _userRepo = userRepo;
        private readonly UserGroupRepo _userGroupRepo = userGroupRepo;
        private readonly FileItemRepo _fileItemRepo = fileItemRepo;
        private readonly MaterialRepo _materialRepo = materialRepo;
        private readonly FileDirRepo _fileDirRepo = fileDirRepo;
        private readonly IStorage _storage = storage;

        public QuickSearchResult SearchWikiItem(string str, bool isAdmin)
        {
            var q = _wikiItemRepo.QuickSearch(str, isAdmin);
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
            var q = _userRepo.QuickSearch(str);
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
            var q = _userGroupRepo.QuickSearch(str);
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
            var q = _fileItemRepo.QuickSearch(str);
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
            var q = _materialRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Id, x.Name, x.StorePathName }).Take(maxCount).ToList();
            QuickSearchResult res = new(true);
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Name ?? "N/A", _storage.FullUrl(x.StorePathName ?? "??"), x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchFileDir(string str)
        {
            var dirs = _fileDirRepo.QuickSearch(str).Select(x => new { x.Id, x.Name }).Take(maxCount).ToList();
            var nameChains = _fileDirRepo.GetNameChainsByIds(dirs.ConvertAll(x => x.Id));
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
