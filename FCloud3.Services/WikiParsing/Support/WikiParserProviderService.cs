using FCloud3.Entities.Wiki;
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
        ILogger<WikiParserProviderService> logger)
    {
        public Parser Get(string cacheKey, bool saveParser,
            Action<ParserBuilder>? configure = null, List<WikiTitleContain>? containInfos = null,
            bool linkSingle = true, Func<int[]>? getTitleContainExpiringWikiIds = null)
        {
            if (!saveParser || cache.Get(cacheKey) is not Parser p)
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
                var changeToken = new CancellationChangeToken(token);
                p = Get(configure, containInfos, linkSingle);
                string logDesc = saveParser ? "缓存之" : "不缓存之";
                logger.LogDebug("词条解析器[{cacheKey}]已创建，{logDesc}", cacheKey, logDesc);

                if (saveParser)
                {
                    var options = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                        .AddExpirationToken(changeToken);
                    options.RegisterPostEvictionCallback((a, b, c, d) =>
                    {
                        logger.LogDebug("词条解析器[{cacheKey}]缓存被过期", cacheKey);
                    });
                    cache.Set(cacheKey, p, options);
                }
            }
            return p;
        }
        private Parser Get(
            Action<ParserBuilder>? configure = null, 
            List<WikiTitleContain>? containInfos = null, 
            bool linkSingle = true)
        {
            var pb = new ParserBuilder();
            if (containInfos != null)
            {
                //自动链接，需要在词条包含标题信息有变更时丢弃缓存
                var containWikiIds = containInfos.ConvertAll(x => x.WikiId);
                var wikiTitles = wikiItemRepo.CachedItemsPropByIds(containWikiIds, w => w.Title);
                pb.AutoReplace.AddReplacingTargets(wikiTitles, linkSingle);
            }
            pb.Link.ReplaceConvertFn((link, name) =>
            {
                name ??= link.Text;
                return WikiParserDataSource.WikiReplacement(link.Url, name);
            });
            if (configure is not null)
                configure(pb);
            Parser parser = pb.BuildParser();
            return parser;
        }
    }
}
