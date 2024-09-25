using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Options.SubOptions;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    /// <summary>
    /// “缓存”上下文<br/>
    /// 在预览等需要频繁解析同一段文本（只有少数改动）时，可通过设置传入的CacheOptions启用缓存<br/>
    /// 相同的文本片段会在这里找到结果，直接返回，避免重复解析<br/>
    /// 在需要“一次性规则”的场合下不可启用缓存
    /// </summary>
    public class ParserCacheContext
    {
        public int CacheReadCount { get; private set; }
        private readonly IMemoryCache? _cache;
        private readonly CacheOptions _options;
        private readonly ParserContext _ctx;
        private readonly StringBuilder _tempSb;
        public ParserCacheContext(CacheOptions options, ParserContext ctx)
        {
            _options = options;
            _ctx = ctx;
            _tempSb = new();
            if (options.CacheInstance is null && options.UseCache)
                throw new InvalidOperationException("未提供缓存实例");
            _cache = options.CacheInstance;
        }
        
        public void Reset()
        {
            CacheReadCount = 0;
            ParsedSavedScanChar = 0;
        }

        public int ParsedSavedScanChar { get; private set; }

        private int CacheKey(string input) => GetHashCode() + input.GetHashCode();
        private void SaveParseResult(string input, string output, 
            List<IRule>? usedRules, List<IHtmlable>? footNotes, List<ParserTitleTreeNode>? titleNodes)
        {
            if (string.IsNullOrEmpty(input) || _cache is null)
                return;
            var cache = new CacheValue(output, usedRules, footNotes, titleNodes);

            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_options.ExpToken);
            var token = new CancellationChangeToken(tokenSource.Token);
            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(_options.SlideExpirationMins))
                .SetPriority(CacheItemPriority.Low)
                .AddExpirationToken(token);
            _cache.Set(CacheKey(input), cache, options);
        }
        public CachedElement SaveParsedElement(string input, IHtmlable resElement)
        {
            _tempSb.Clear();
            resElement.WriteHtml(_tempSb);
            string content = _tempSb.ToString();
            _tempSb.Clear();
            List<IRule>? usedRules = resElement.ContainRules();
            List<IHtmlable>? footNotes = resElement.ContainFootNotes();
            List<ParserTitleTreeNode>? titleNodes = _ctx.Options.TitleGatheringOptions.Enabled ? resElement.ContainTitleNodes() : null;
            
            var element = new CachedElement(content, usedRules, footNotes, titleNodes);
            if (usedRules is not null && usedRules.Any(r => _options.NoCacheRules.Contains(r.UniqueName)))
                return element;
            SaveParseResult(input, content, usedRules, footNotes, titleNodes);
            return element;
        }
        private CacheValue? ReadParseResult(string input)
        {
            var cache = _cache?.Get<CacheValue>(CacheKey(input));
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
                //有缓存的地方没法确保单次使用
                
                _ctx.RuleUsage.ReportUsage(res.UsedRules);
                _ctx.FootNote.AddFootNoteBodies(res.FootNotes);
                return new CachedElement(res.Content, res.UsedRules, res.FootNotes, res.TitleNodes);
            }
            return null;
        }

        public class CacheValue
        {
            public string Content { get; }
            public List<IRule>? UsedRules { get; }
            public List<IHtmlable>? FootNotes { get; }
            public List<ParserTitleTreeNode>? TitleNodes { get; }
            public CacheValue(string content, List<IRule>? usedRules, List<IHtmlable>? footNotes, List<ParserTitleTreeNode>? titleNodes)
            {
                Content = content;
                UsedRules = usedRules;
                FootNotes = footNotes;
                TitleNodes = titleNodes;
            }
        }

        public string DebugInfo()
        {
            string info = "";
            if (_options.UseCache)
            {
                info += $"缓存被读取{CacheReadCount}次，避免{ParsedSavedScanChar}字符被重新解析";
                var s = _cache?.GetCurrentStatistics();
                if (s is not null)
                {
                    info += $"<br/> EntryCount：{s.CurrentEntryCount}";
                }
            }
            else
            {
                info += "未开启缓存";
            }
            return HtmlLabel.DebugInfo(info);
        }
    }
}
