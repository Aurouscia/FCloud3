using FCloud3.Entities.Wiki;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using Microsoft.Extensions.Logging;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc.Cache;

namespace FCloud3.Services.WikiParsing.Support
{
    public class WikiParserProviderService(
        WikiItemRepo wikiItemRepo,
        WikiParserCacheHost wikiParserCacheHost,
        ILogger<WikiParserProviderService> logger)
    {
        public Parser Get(string cacheKey, bool saveParser,
            Action<ParserBuilder>? configure = null, Func<List<WikiTitleContain>>? getContains = null,
            bool linkSingle = true, Func<int[]>? getTitleContainExpiringWikiIds = null)
        {
            if (!saveParser || !wikiParserCacheHost.TryGet(cacheKey, out var p) || p is null)
            {
                p = Get(configure, getContains, linkSingle);
                string logDesc = saveParser ? "缓存之" : "不缓存之";
                logger.LogDebug("词条解析器[{cacheKey}]已创建，{logDesc}", cacheKey, logDesc);
                if (saveParser)
                    wikiParserCacheHost.Set(cacheKey, p);
            }
            else
            {
                logger.LogDebug("词条解析器[{cacheKey}]缓存命中", cacheKey);
            }
            return p;
        }
        private Parser Get(
            Action<ParserBuilder>? configure = null, 
            Func<List<WikiTitleContain>>? getContains = null, 
            bool linkSingle = true)
        {
            var pb = new ParserBuilder();
            if (getContains != null)
            {
                var contains = getContains();
                var containWikiIds = contains.ConvertAll(x => x.WikiId);
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
