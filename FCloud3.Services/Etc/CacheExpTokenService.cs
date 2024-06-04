using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FCloud3.Services.Etc
{
    public class CacheExpTokenService
    {
        public CacheExpTokenService(ILogger<CacheExpTokenService> logger)
        {
            WikiTitleContain = new("词条标题包含", logger);
            WikiItemNamePathInfo = new("词条名称和路径名", logger);
            MaterialNamePathInfo = new("素材名称和路径名", logger);
            AuthGrants = new("权限授予对象", logger);
        }
        /// <summary>
        /// “词条标题包含”的缓存过期token提供源，key为wikiId，
        /// 当某段落的“词条标题包含”有改动时，必须将包含其的词条的wikiId对应的token过期，使解析器重新构造
        /// </summary>
        public CacheExpTokenManagerCollection WikiTitleContain { get; }
        /// <summary>
        /// “全部词条名称和路径名”的缓存过期token提供源
        /// 当任意词条的名称或路径名有改动时，必须将对应的token过期，使解析器重新构造
        /// </summary>
        public CacheExpTokenManager WikiItemNamePathInfo { get; }
        /// <summary>
        /// “全部素材名称和路径名”的缓存过期token提供源
        /// 当任意素材的名称或路径名有改动时，必须将对应的token过期，使解析器重新构造
        /// </summary>
        public CacheExpTokenManager MaterialNamePathInfo { get; }
        /// <summary>
        /// “全部权限授予对象”的缓存过期token提供源
        /// 当任意权限授予对象改动，创建或删除时，必须将对应token过期，使“是否通过验证”缓存过期
        /// </summary>
        public CacheExpTokenManager AuthGrants { get; }

        public class CacheExpTokenManager
        {
            public CacheExpTokenManager(string? name = null, ILogger<CacheExpTokenService>? logger = null)
            {
                Name = name ?? "CacheExpTokenManager";
                TokenSources = [];
                Logger = logger;
            }

            public string Name { get; }
            private List<CancellationTokenSource> TokenSources { get; }
            private ILogger<CacheExpTokenService>? Logger { get; }
            public CancellationToken GetCancelToken()
            {
                CancellationTokenSource tokenSource = new();
                TokenSources.Add(tokenSource);
                return tokenSource.Token;
            }
            public CancellationChangeToken GetCancelChangeToken()
            {
                var t = GetCancelToken();
                return new(t);
            }
            public void CancelAll()
            {
                TokenSources.ForEach(t => t.Cancel());
                TokenSources.Clear();
                Logger?.LogInformation("关于[{Name}]的缓存已全部丢弃", Name);
            }
        }

        public class CacheExpTokenManagerCollection : Dictionary<int, CacheExpTokenManager>
        {
            public string Name { get; }
            private ILogger<CacheExpTokenService>? Logger { get; }
            public CacheExpTokenManagerCollection(string? name = null, ILogger<CacheExpTokenService>? logger = null)
            {
                Name = name ?? "CacheExpTokenManager";
                Logger = logger;
            }
            public CacheExpTokenManager GetByKey(int key)
            {
                if (TryGetValue(key, out var manager))
                {
                    return manager;
                }
                else
                {
                    var newM = new CacheExpTokenManager($"{Name}_{key}", Logger);
                    this[key] = newM;
                    return newM;
                }
            }
            public CancellationToken[] GetTokensOfAll(int[] keys)
            {
                CancellationToken[] tokens = new CancellationToken[keys.Length];
                for(int i = 0; i<tokens.Length; i++)
                {
                    tokens[i] = GetByKey(keys[i]).GetCancelToken();
                }
                return tokens;
            }
            public CancellationToken GetLinkedTokenOfAll(int[] keys)
            {
                var tokens = GetTokensOfAll(keys);
                var source = CancellationTokenSource.CreateLinkedTokenSource(tokens);
                return source.Token;
            }
        }
    }
}
