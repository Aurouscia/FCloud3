using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class CacheOptions
    {
        public bool UseCache { get; private set; }
        public float SlideExpirationMins { get; private set; }
        public IMemoryCache? CacheInstance { get; private set; }
        public List<string> NoCacheRules { get; private set; }
        public CancellationToken ExpToken { get; private set; }

        private readonly ParserBuilder _master;

        public CacheOptions(ParserBuilder master)
        {
            UseCache = true;
            SlideExpirationMins = 1;
            _master = master;
            NoCacheRules = new List<string>();
        }
        public ParserBuilder DisableCache()
        {
            UseCache = false;
            return _master;
        }
        public ParserBuilder SetSlideExpirationMins(float mins)
        {
            SlideExpirationMins = mins;
            return _master;
        }
        public ParserBuilder UseCacheInstance(IMemoryCache cacheInstance)
        {
            CacheInstance = cacheInstance;
            return _master;
        }
        public ParserBuilder SetNoCacheRules(List<string> strs)
        {
            NoCacheRules = NoCacheRules.Union(strs).ToList();
            return _master;
        }

        public ParserBuilder SetExpireToken(CancellationToken token)
        {
            ExpToken = token;
            return _master;
        }
    }
}
