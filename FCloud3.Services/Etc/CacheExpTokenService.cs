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
            MaterialInfo = new("素材信息", logger);
            UserTypeInfo = new("用户类型", logger);
        }

        public CacheExpTokenManager WikiTitleContain { get; }
        public CacheExpTokenManager WikiItemNamePathInfo { get; }
        public CacheExpTokenManager MaterialInfo { get; }
        public CacheExpTokenManagerCollection UserTypeInfo { get; }

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
            public CancellationToken[] GetTokenOfAll(int[] keys)
            {
                CancellationToken[] tokens = new CancellationToken[keys.Length];
                for(int i = 0; i<tokens.Length; i++)
                {
                    tokens[i] = GetByKey(keys[i]).GetCancelToken();
                }
                return tokens;
            }
        }
    }
}
