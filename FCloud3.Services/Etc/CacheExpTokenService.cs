﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FCloud3.Services.Etc
{
    [Obsolete("暂时用不到")]
    public class CacheExpTokenService
    {
        public CacheExpTokenService(ILogger<CacheExpTokenService> logger)
        {

        }

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
