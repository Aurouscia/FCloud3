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

        public QuickSearchService(
            WikiItemRepo wikiItemRepo)
        {
            _wikiItemRepo = wikiItemRepo;
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
