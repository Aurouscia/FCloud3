using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace FCloud3.HtmlGen.Context.SubContext
{
    public class ParserCacheContext : IDisposable
    {
        public int CacheReadCount { get; private set; }
        private readonly IMemoryCache _cache;
        private readonly CacheOptions _options;
        private readonly ParserContext _ctx;
        private readonly StringBuilder _tempSb;

        public bool IsSelfHostedCache { get; }
        public ParserCacheContext(CacheOptions options, ParserContext ctx)
        {
            _options = options;
            _ctx = ctx;
            _tempSb = new();
            //未提供缓存实例，自己造，自己回收
            if (options.CacheInstance is null)
            {
                IsSelfHostedCache = true;
                _cache = new MemoryCache(new MemoryCacheOptions());
            }
            else
            {
                IsSelfHostedCache = false;
                _cache = options.CacheInstance;
            }
        }

        ~ParserCacheContext()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (IsSelfHostedCache)
            {
                _cache.Dispose();
                GC.SuppressFinalize(this);
            }
        }
        public void Reset()
        {
            CacheReadCount = 0;
            ParsedSavedScanChar = 0;
        }

        public int ParsedSavedScanChar { get; private set; }

        private int CacheKey(string input) => GetHashCode() + input.GetHashCode();
        private void SaveParseResult(string input, string output, List<IRule>? usedRules, List<IHtmlable>? footNotes)
        {
            if (string.IsNullOrEmpty(input))
                return;
            var cache = new CacheValue(output, usedRules, footNotes);
            _cache.Set(CacheKey(input), cache, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(_options.SlideExpirationMins),
            });
        }
        public CachedElement SaveParsedElement(string input, IHtmlable resElement)
        {
            _tempSb.Clear();
            resElement.WriteHtml(_tempSb);
            string content = _tempSb.ToString();
            _tempSb.Clear();
            List<IRule>? usedRules = resElement.ContainRules();
            List<IHtmlable>? footNotes = resElement.ContainFootNotes();
            var element = new CachedElement(content, usedRules, footNotes);
            if (usedRules is not null && usedRules.Any(r => _options.NoCacheRules.Contains(r.UniqueName)))
                return element;
            SaveParseResult(input, content, usedRules, footNotes);
            return element;
        }
        private CacheValue? ReadParseResult(string input)
        {
            var cache = _cache.Get<CacheValue>(CacheKey(input));
            if (cache is not null)
            {
                CacheReadCount++;
                ParsedSavedScanChar += input.Length;
            }
            return cache;
        }
        public CachedElement? ReadParsedElement(string input)
        {
            var res = ReadParseResult(input);
            if (res is not null)
            {
                //如果缓存里使用的一次性规则已经在前面被用过了，那么缓存失效，重新算
                if (res.UsedRules is null || !_ctx.RuleUsage.ViolateSingleUsage(res.UsedRules))
                {
                    _ctx.RuleUsage.ReportUsage(res.UsedRules);
                    _ctx.FootNote.AddFootNoteBodies(res.FootNotes);
                    return new CachedElement(res.Content, res.UsedRules, res.FootNotes);
                }
            }
            return null;
        }

        public class CacheValue
        {
            public string Content { get; }
            public List<IRule>? UsedRules { get; }
            public List<IHtmlable>? FootNotes { get; }
            public CacheValue(string content, List<IRule>? usedRules, List<IHtmlable>? footNotes)
            {
                Content = content;
                UsedRules = usedRules;
                FootNotes = footNotes;
            }
        }

        public string DebugInfo()
        {
            string info = "";
            if (IsSelfHostedCache)
                info += "[自托管缓存]";
            else
                info += "[提供的缓存实例]";
            if (_options.UseCache)
            {
                info += $"缓存被读取{CacheReadCount}次，避免{ParsedSavedScanChar}字符被重新解析";
            }
            else
            {
                info += "未开启缓存";
            }
            return HtmlLabel.DebugInfo(info);
        }
    }
}
