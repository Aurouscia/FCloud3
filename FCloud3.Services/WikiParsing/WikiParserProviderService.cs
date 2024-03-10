using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.WikiParsing
{
    public class WikiParserProviderService
    {
        private readonly IMemoryCache _cache;

        public WikiParserProviderService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public Parser Get(string cacheKey, Action<ParserBuilder>? configure = null)
        {
            if (_cache.Get(cacheKey) is not Parser p)
            {
                p = Get(configure);
                _cache.Set(cacheKey, p, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }
            return p;
        }
        private Parser Get(Action<ParserBuilder>? configure = null)
        {
            ParserBuilder pb = new();
            pb.Cache.UseCacheInstance(_cache);
            pb.Block.SetTitleLevelOffset(1);
            pb.TitleGathering.Enable();
            if (configure is not null)
                configure(pb);
            Parser parser = pb.BuildParser();
            return parser;
        }
    }
}
