using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Util;
using FCloud3.App.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.App.Services
{
    public class HtmlGenParserProvider
    {
        private readonly IMemoryCache _cache;
        private readonly HttpUserInfoService _userInfo;

        public HtmlGenParserProvider(IMemoryCache cache,HttpUserInfoService userInfo)
        {
            _cache = cache;
            _userInfo = userInfo;
        }
        public Parser GetParser(int textSecId)
        {
            var cache = _cache.Get<Parser>(Key(textSecId));
            if (cache is not null)
                return cache;
            var p = BuildParser();
            var entryOption = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
            };
            entryOption.RegisterPostEvictionCallback((key,value,reason,state) =>
            {
                if (value is Parser parser)
                    parser.Dispose();
            });
            _cache.Set<Parser>(Key(textSecId), p, entryOption);
            return p;
        }
        private Parser BuildParser()
        {
            return new ParserBuilder()
                .EnableDebugInfo()
                .UseLocatorHash(new LocatorHash())
                .Cache.UseCacheInstance(_cache)
                .Block.SetTitleLevelOffset(1)
                .BuildParser();
        }
        private string Key(int textSecId)
        {
            return $"hgParser_{_userInfo.Id}_{textSecId}";
        }

        public class LocatorHash : ILocatorHash
        {
            public string? Hash(string? input)
            {
                return MD5Helper.GetMD5Of($"locHash_{input?.Trim()}");
            }
        }
    }
}
