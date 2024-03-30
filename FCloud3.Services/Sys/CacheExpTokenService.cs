using Microsoft.Extensions.Primitives;

namespace FCloud3.Services.Sys
{
    public class CacheExpTokenService
    {
        public CacheExpTokenService()
        {
            WikiTitleContain = new();
            WikiItemInfo = new();
        }

        public CacheExpTokenManager WikiTitleContain { get; }
        public CacheExpTokenManager WikiItemInfo { get; }

        public class CacheExpTokenManager
        {
            public CacheExpTokenManager()
            {
                TokenSources = new();
            }
            private List<CancellationTokenSource> TokenSources { get; }
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
            }
        }
    }
}
