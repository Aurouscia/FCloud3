using FCloud3.Entities.Wiki;
using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.Services.Etc;
using FCloud3.Services.Files;
using FCloud3.Services.Wiki;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace FCloud3.Services.WikiParsing.Support
{
    public class WikiParserProviderService
    {
        private readonly IMemoryCache _cache;
        private readonly CacheExpTokenService _cacheExpTokenService;
        private readonly WikiItemService _wikiItemService;
        private readonly MaterialService _materialService;

        public WikiParserProviderService(
            IMemoryCache cache,
            CacheExpTokenService cacheExpTokenService,
            WikiItemService wikiItemService,
            MaterialService materialService)
        {
            _cache = cache;
            _cacheExpTokenService = cacheExpTokenService;
            _wikiItemService = wikiItemService;
            _materialService = materialService;
        }
        public Parser Get(string cacheKey, Action<ParserBuilder>? configure = null, List<WikiTitleContain>? containInfos = null, bool linkSingle = true)
        {
            if (_cache.Get(cacheKey) is not Parser p)
            {
                var titleContainToken = _cacheExpTokenService.WikiTitleContain.GetCancelToken();
                var wikiItemInfoToken = _cacheExpTokenService.WikiItemInfo.GetCancelToken();
                var materialInfoToken = _cacheExpTokenService.MaterialInfo.GetCancelToken();
                var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(titleContainToken, wikiItemInfoToken, materialInfoToken);
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
            var allWikis = _wikiItemService.WikiItemsMetaAll();
            var allMeterials = _materialService.AllMaterialsMeta();
            if (containInfos != null)
            {
                //自动链接，需要在词条包含标题信息有变更时丢弃缓存
                var containWikiIds = containInfos.ConvertAll(x => x.WikiId);
                var wikis = allWikis.FindAll(x => containWikiIds.Contains(x.Id));
                Func<string, string> func = (title) =>
                {
                    var w = wikis.FirstOrDefault(x => x.Title == title);
                    if (w != null && w.UrlPathName != null && w.Title != null)
                        return WikiReplacement(w.UrlPathName, w.Title);
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
                    return WikiReplacement(w.UrlPathName, w.Title);
                return x;
            });
            pb.Implant.AddImplantsHandler(x =>
            {
                var parts = x.Split(materialImplantSep);
                var firstPart = parts.ElementAtOrDefault(0);
                var secondPart = parts.ElementAtOrDefault(1);
                var m = allMeterials.FirstOrDefault(m => m.Name == firstPart);
                if (m is not null && m.Name != null && m.PathName != null)
                    return MaterialReplacement(m.PathName, secondPart);
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

        private static string WikiReplacement(string urlPathName, string title) => $"<a pathName=\"{urlPathName}\">{title}</a>";


        private static readonly char[] materialImplantSep = new[] { ':', '：' };
        public string MaterialReplacement(string pathName, string? heightExp)
        {
            if (heightExp is null)
                heightExp = "2rem";
            else if (int.TryParse(heightExp, out int _))
                heightExp += "rem";
            return $"<img class=\"wikiInlineImg\" src=\"{MaterialSrc(pathName)}\" style=\"height:{heightExp}\" />";
        }
        private string MaterialSrc(string pathName) => _materialService.GetMaterialFullSrc(pathName);
    }
}
