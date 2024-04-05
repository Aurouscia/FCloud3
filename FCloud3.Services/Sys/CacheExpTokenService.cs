using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FCloud3.Services.Sys
{
    public class CacheExpTokenService
    {
        public CacheExpTokenService(ILogger<CacheExpTokenManager> logger)
        {
            WikiTitleContain = new(nameof(WikiTitleContain), logger);
            WikiItemInfo = new(nameof(WikiTitleContain), logger);
            MaterialInfo = new(nameof(MaterialInfo), logger);
        }

        public CacheExpTokenManager WikiTitleContain { get; }
        public CacheExpTokenManager WikiItemInfo { get; }
        public CacheExpTokenManager MaterialInfo { get; }

        public class CacheExpTokenManager
        {
            public CacheExpTokenManager(string? name = null, ILogger<CacheExpTokenManager>? logger = null)
            {
                Name = name ?? "CacheExpTokenManager";
                TokenSources = [];
                Logger = logger;
            }

            public string Name { get; }
            private List<CancellationTokenSource> TokenSources { get; }
            private ILogger<CacheExpTokenManager>? Logger { get; }
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
                TokenSources.ForEach(t => t.Cancel()) ;
                TokenSources.Clear();
                Logger?.LogInformation("关于_{Name}_的缓存已全部丢弃", Name);
            }
        }
    }
}
