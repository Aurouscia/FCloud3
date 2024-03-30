using FCloud3.Entities.Wiki;
using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.Repos.Wiki;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.WikiParsing.Support
{
    public class WikiParserProviderService
    {
        private readonly IMemoryCache _cache;
        private readonly WikiTitleContainRepo _wikiTitleContainRepo;
        private readonly WikiItemRepo _wikiItemRepo;

        public WikiParserProviderService(
            IMemoryCache cache,
            WikiTitleContainRepo wikiTitleContainRepo,
            WikiItemRepo wikiItemRepo)
        {
            _cache = cache;
            _wikiTitleContainRepo = wikiTitleContainRepo;
            _wikiItemRepo = wikiItemRepo;
        }
        public Parser Get(string cacheKey, Action<ParserBuilder>? configure = null, List<WikiTitleContain>? containInfos = null)
        {
            if (_cache.Get(cacheKey) is not Parser p)
            {
                p = Get(configure, containInfos);
                _cache.Set(cacheKey, p, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }
            return p;
        }
        private Parser Get(Action<ParserBuilder>? configure = null, List<WikiTitleContain>? containInfos = null)
        {
            var pb = DefaultConfigureBuilder();
            var allWikis = _wikiItemRepo.Existing.Select(x => new { x.Id, x.UrlPathName, x.Title }).ToList();
            if (containInfos != null)
            {
                var containWikiIds = containInfos.ConvertAll(x => x.WikiId);
                var wikis = allWikis.FindAll(x => containWikiIds.Contains(x.Id));
                Func<string, string> func = (title) =>
                {
                    var w = wikis.FirstOrDefault(x => x.Title == title);
                    if (w != null && w.UrlPathName != null && w.Title != null)
                        return WikiLink(w.UrlPathName, w.Title);
                    return title;
                };
                var reps = wikis.Where(x => x.Title != null).Select(x => x.Title).ToList();
                pb.AutoReplace.AddReplacing(reps!, func);
            }
            pb.Implant.AddImplantsHandler(x =>
            {
                var w = allWikis.FirstOrDefault(w => w.UrlPathName == x);
                if (w is not null && w.UrlPathName != null && w.Title != null)
                    return WikiLink(w.UrlPathName, w.Title);
                return x;
            });
            if (configure is not null)
                configure(pb);
            Parser parser = pb.BuildParser();
            return parser;
        }
        private ParserBuilder DefaultConfigureBuilder()
        {
            ParserBuilder pb = new();
            pb.Cache.UseCacheInstance(_cache);
            pb.Block.SetTitleLevelOffset(1);
            pb.TitleGathering.Enable();
            return pb;
        }

        //public void ConfigureWikiLink(ParserBuilder pb, WikiTitleContainType type, List<int> objectIds, Func<string, string, string> wikiLink)
        //{
        //    var contains = _wikiTitleContainRepo.GetIdsByTypeAndObjIds(type, objectIds);
        //    var wikis = _wikiItemRepo.GetRangeByIds(contains).Select(x => new { x.UrlPathName, x.Title }).ToList();
        //    Func<string, string> func = (title) =>
        //    {
        //        var w = wikis.FirstOrDefault(x => x.Title == title);
        //        if (w != null && w.UrlPathName != null && w.Title != null)
        //            return wikiLink(w.UrlPathName, w.Title);
        //        return title;
        //    };
        //    var reps = wikis.Where(x=>x.Title != null).Select(x=>x.Title).ToList();
        //    pb.AutoReplace.AddReplacing(reps!, func);
        //}

        //public void ConfigureReplacement(ParserBuilder pb, Func<string, string, string> wikiLink)
        //{
        //    var wikis = _wikiItemRepo.Existing.Select(x => new { x.UrlPathName, x.Title });
        //    pb.Implant.AddImplantsHandler(x =>
        //    {
        //        var w = wikis.FirstOrDefault(w => w.UrlPathName == x);
        //        if(w is not null && w.UrlPathName != null && w.Title != null)
        //            return wikiLink(w.UrlPathName, w.Title);
        //        return x;
        //    });
        //}

        public static string WikiLink(string urlPathName, string title) => $"<a pathName=\"{urlPathName}\">{title}</a>";
    }
}
