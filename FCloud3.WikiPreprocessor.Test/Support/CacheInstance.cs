using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.WikiPreprocessor.Test.Support
{
    public static class CacheInstance
    {
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        public static IMemoryCache Get() => _cache;
    }
}