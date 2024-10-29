﻿using FCloud3.Entities.Wiki;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.Services.Etc;
using FCloud3.Services.Files;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.Files;

namespace FCloud3.Services.WikiParsing.Support
{
    public class WikiParserProviderService(
        IMemoryCache cache,
        CacheExpTokenService cacheExpTokenService,
        WikiItemRepo wikiItemRepo,
        MaterialService materialService,
        MaterialRepo materialRepo,
        ILogger<WikiParserProviderService> logger)
    {
        public Parser Get(string cacheKey, List<WikiItemCacheModel>? allWikis = null,
            Action<ParserBuilder>? configure = null, List<WikiTitleContain>? containInfos = null,
            bool linkSingle = true, bool autoreplaceOnlyFunc = false, Func<int[]>? getTitleContainExpiringWikiIds = null)
        {
            if (cache.Get(cacheKey) is not Parser p)
            {
                CancellationToken titleContainToken;
                if (getTitleContainExpiringWikiIds is not null)
                    titleContainToken = cacheExpTokenService.WikiTitleContain.GetLinkedTokenOfAll(getTitleContainExpiringWikiIds());
                else
                    titleContainToken = new(false);
                var wikiItemInfoToken = cacheExpTokenService.WikiItemNamePathInfo.GetCancelToken();
                var materialInfoToken = cacheExpTokenService.MaterialNamePathInfo.GetCancelToken();
                var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(titleContainToken, wikiItemInfoToken, materialInfoToken);
                var token = linkedSource.Token;
                Action<ParserBuilder> configureAndToken = b => {
                    if(configure is not null)
                        configure(b);
                    b.Cache.SetExpireToken(token);
                };
                var changeToken = new CancellationChangeToken(token);
                p = Get(allWikis, configureAndToken, containInfos, linkSingle, autoreplaceOnlyFunc);
                logger.LogInformation("词条解析器[{cacheKey}]已创建", cacheKey);
                var options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .AddExpirationToken(changeToken);
                options.RegisterPostEvictionCallback((a, b, c, d) =>
                {
                    logger.LogInformation("词条解析器[{cacheKey}]缓存被过期", cacheKey);
                });
                cache.Set(cacheKey, p, options);
            }
            return p;
        }
        private Parser Get(
            List<WikiItemCacheModel>? allWikis = null, 
            Action<ParserBuilder>? configure = null, 
            List<WikiTitleContain>? containInfos = null, 
            bool linkSingle = true,
            bool autoreplaceOnlyFunc = false)
        {
            var pb = DefaultConfigureBuilder();
            allWikis ??= wikiItemRepo.CachedItemsByPred(x => !x.Sealed).ToList();
            var allMeterials = materialRepo.AllCachedItems();
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
                List<string?> targets = [];
                if (!autoreplaceOnlyFunc)
                {
                    targets = wikis.Select(x => x.Title).ToList();
                    targets.RemoveAll(x => x is null);
                }
                pb.AutoReplace.AddReplacing(targets!, func, linkSingle);
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
            allWikis.ForEach(w =>
            {
                if(w.Title != null && w.UrlPathName != null)
                    pb.Link.AddLinkItem(w.Title, w.UrlPathName);
            });
            pb.Link.ReplaceConvertFn((link, name) =>
            {
                name ??= link.Text;
                return WikiReplacement(link.Url, name);
            });
            if (configure is not null)
                configure(pb);
            Parser parser = pb.BuildParser();
            return parser;
        }
        private ParserBuilder DefaultConfigureBuilder()
        {
            ParserBuilder pb = new();
            pb.Cache.UseCacheInstance(cache);
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
            else if (float.TryParse(heightExp, out float _))
                heightExp += "rem";
            return $"<img class=\"wikiInlineImg\" src=\"{MaterialSrc(pathName)}\" style=\"height:{heightExp}\" />";
        }
        private string MaterialSrc(string pathName) => materialService.GetMaterialFullSrc(pathName);
    }
}
