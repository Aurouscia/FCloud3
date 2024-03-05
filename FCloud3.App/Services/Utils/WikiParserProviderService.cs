using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Util;
using FCloud3.App.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.App.Services.Utils
{
    public class WikiParserProviderService
    {
        private readonly IMemoryCache _cache;
        private readonly ILocatorHash _locatorHash;

        public WikiParserProviderService(IMemoryCache cache, ILocatorHash locatorHash)
        {
            _cache = cache;
            _locatorHash = locatorHash;
        }
        public Parser GetParser(string cacheKey, Func<ParserBuilder, ParserBuilder>? configure = null)
        {
            var cache = _cache.Get<Parser>(Key(cacheKey));
            if (cache is not null)
                return cache;
            var p = BuildParser(configure);
            var entryOption = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
            };
            entryOption.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                if (value is Parser parser)
                    parser.Dispose();
            });
            _cache.Set(Key(cacheKey), p, entryOption);
            return p;
        }
        private Parser BuildParser(Func<ParserBuilder, ParserBuilder>? configure = null)
        {
            var builder = new ParserBuilder();
            builder
                .EnableDebugInfo()
                .UseLocatorHash(_locatorHash)
                .Cache.UseCacheInstance(_cache)
                .Block.SetTitleLevelOffset(1);
            if (configure is not null)
                configure(builder);
            return builder.BuildParser();
        }
        private static string Key(string cacheKey) => $"hgParser_{cacheKey}";
    }
}
