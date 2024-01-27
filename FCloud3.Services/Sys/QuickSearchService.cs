using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Sys
{
    public class QuickSearchService
    {
        private const int maxCount = 8;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly UserRepo _userRepo;
        private readonly FileItemRepo _fileItemRepo;

        public QuickSearchService(
            WikiItemRepo wikiItemRepo,
            UserRepo userRepo,
            FileItemRepo fileItemRepo)
        {
            _wikiItemRepo = wikiItemRepo;
            _userRepo = userRepo;
            _fileItemRepo = fileItemRepo;
        }

        public QuickSearchResult SearchWikiItem(string str)
        {
            var q = _wikiItemRepo.QuickSearch(str);
            var items =  q.Select(x => new { x.Title,x.UrlPathName, x.Id }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Title??"N/A",x.UrlPathName ,x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchUser(string str)
        {
            var q = _userRepo.QuickSearch(str);
            var items = q.Select(x => new {x.Name,x.Id}).Take(maxCount).ToList();
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
    }

    public class QuickSearchResult
    {
        public List<QuickSearchResultItem> Items { get; set; }
        public QuickSearchResult()
        {
            Items = new();
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
