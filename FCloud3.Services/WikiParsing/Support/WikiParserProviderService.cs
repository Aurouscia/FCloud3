using FCloud3.Entities.Wiki;
using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Sys;
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
        private readonly CacheExpTokenService _cacheExpTokenService;
        private readonly WikiItemRepo _wikiItemRepo;

        public WikiParserProviderService(
            IMemoryCache cache,
            CacheExpTokenService cacheExpTokenService,
            WikiItemRepo wikiItemRepo)
        {
            _cache = cache;
            _cacheExpTokenService = cacheExpTokenService;
            _wikiItemRepo = wikiItemRepo;
        }
        public Parser Get(string cacheKey, Action<ParserBuilder>? configure = null, List<WikiTitleContain>? containInfos = null, bool linkSingle = true)
        {
            if (_cache.Get(cacheKey) is not Parser p)
            {
                var titleContainToken = _cacheExpTokenService.WikiTitleContain.GetCancelToken();
                var wikiItemInfoToken = _cacheExpTokenService.WikiItemInfo.GetCancelToken();
                var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(titleContainToken, wikiItemInfoToken);
                var token = linkedSource.Token;
                Action<ParserBuilder> configureAndToken = b => {
                    if(configure is not null)
                        configure(b);
                    b.Cache.SetExpireToken(token);
                };
                var changeToken = new CancellationChangeToken(token);
                p = Get(configureAndToken, containInfos, linkSingle);
                var options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .AddExpirationToken(changeToken);
                _cache.Set(cacheKey, p, options);
            }
            return p;
        }
        private Parser Get(Action<ParserBuilder>? configure = null, List<WikiTitleContain>? containInfos = null, bool linkSingle = true)
        {
            var pb = DefaultConfigureBuilder();
            var allWikis = _wikiItemRepo.Existing.Select(x => new { x.Id, x.UrlPathName, x.Title }).ToList();
            if (containInfos != null)
            {
                //自动链接，需要在词条包含标题信息有变更时丢弃缓存
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
                pb.AutoReplace.AddReplacing(reps!, func, linkSingle);
            }
            pb.Implant.AddImplantsHandler(x =>
            {
                //词条路径名嵌入，需要在词条信息有变更时丢弃缓存
                var xTrimmed = x.Trim();
                var w = allWikis.FirstOrDefault(w => w.UrlPathName == xTrimmed || w.Title == xTrimmed);
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

        public static string WikiLink(string urlPathName, string title) => $"<a pathName=\"{urlPathName}\">{title}</a>";
    }
}
