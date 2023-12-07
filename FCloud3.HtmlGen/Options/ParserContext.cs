using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    /// <summary>
    /// Html生成器上下文，在单次解析中唯一
    /// </summary>
    public class ParserContext:IDisposable
    {
        public ParserOptions Options { get; }
        public TemplateSlotInfo TemplateSlotInfo { get; }
        public ParserCacheContext Caches { get; }
        public ParserRuleUsageContext RuleUsage { get; }

        /// <summary>
        /// 用于模板中需要产生唯一标识符处，使用一次自增一次
        /// </summary>
        public int UniqueSlotIncre { get; set; }
        public ParserContext(ParserOptions options)
        {
            RuleUsage = new();
            TemplateSlotInfo = new();
            Options = options;
            Caches = new(options.CacheOptions);
        }
        /// <summary>
        /// 在同一个Parser对象多次反复运行之间，将一些参数设回初始值。
        /// </summary>
        public void Reset()
        {
            UniqueSlotIncre = 0;
            RuleUsage.Reset();
            Caches.Reset();
        }
        public string DebugInfo()
        {
            return Caches.DebugInfo()+RuleUsage.DebugInfo();
        }
        ~ParserContext()
        {
            Dispose();
        }
        public void Dispose()
        {
            Caches.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    public class ParserRuleUsageContext
    {
        public Dictionary<IRule,int> UsedRulesLog { get; }

        public ParserRuleUsageContext()
        {
            UsedRulesLog = new();
        }
        public void ReportUsage(IRule rule)
        {
            UsedRulesLog.TryAdd(rule, 0);
            UsedRulesLog[rule] += 1;
        }
        public void ReportUsage(List<IRule> rules)
        {
            rules.ForEach(ReportUsage);
        }
        public List<IRule> GetUsedRules()
        {
            return UsedRulesLog.Keys.ToList();
        }
        public int RuleUsedTime(IRule rule)
        {
            _ = UsedRulesLog.TryGetValue(rule, out int times);
            return times;
        }
        public string DebugInfo()
        {
            int rulesCount = UsedRulesLog.Count;
            int totalUsage = UsedRulesLog.Values.Sum();
            string info = $"共使用{rulesCount}种规则，共{totalUsage}处";
            return HtmlLabel.DebugInfo(info);
        }
        public void Reset()
        {
            UsedRulesLog.Clear();
        }
    }
    public class ParserCacheContext:IDisposable
    {
        public int CacheReadCount { get; private set; }
        private MemoryCache _exclusiveCache { get;}
        private MemoryCache _inclusiveCache { get; }
        private readonly CacheOptions _options;
        public ParserCacheContext(CacheOptions options) 
        {
            NonMarkStr = new();   
            NonTemplateStr = new();
            _options = options;
            _exclusiveCache = new(new MemoryCacheOptions());
            _inclusiveCache = new(new MemoryCacheOptions());
        }

        ~ParserCacheContext()
        {
            Dispose();
        }
        public void Dispose()
        {
            _exclusiveCache.Dispose();
            _inclusiveCache.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            NonMarkStr.Clear();
            NonTemplateStr.Clear();
            _exclusiveCache.Clear();
            _inclusiveCache.Clear();
            Reset();
        }
        public void Reset()
        {
            CacheReadCount = 0;
            NonMarkSavedScanChar = 0;
            NonTemplateSavedScanChar = 0;
            ParsedSavedScanChar = 0;
        }

        #region exclusiveCache=========
        private HashSet<int> NonMarkStr { get; }
        public int NonMarkSavedScanChar { get; private set; }
        public void ReportNonMarkString(string str)
        {
            NonMarkStr.Add(str.GetHashCode());
        }
        public bool IsNonMarkString(string str)
        {
            var res = NonMarkStr.Contains(str.GetHashCode());
            if (res)
            {
                CacheReadCount++;
                NonMarkSavedScanChar += str.Length;
            }
            return res;
        }

        private HashSet<int> NonTemplateStr { get; }
        public int NonTemplateSavedScanChar { get; private set; }
        public void ReportNonTemplateStr(string str)
        {
            NonTemplateStr.Add(str.GetHashCode());
        }
        public bool IsNonTemplateStr(string str)
        {
            var res = NonTemplateStr.Contains(str.GetHashCode());
            if (res)
            {
                CacheReadCount++;
                NonTemplateSavedScanChar += str.Length;
            }
            return res;
        }

        public int MarkCacheSavedScanChar { get; private set; }
        public void SetInlineMarks(string input,InlineMarkList marks)
        {
            var copy = new InlineMarkList(marks, 0);
            _exclusiveCache.Set<InlineMarkList>(input.GetHashCode(), copy, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(_options.SlideExpirationMins)
            });
        }
        public InlineMarkList? GetInlineMarks(string input)
        {
            var res = _exclusiveCache.Get<InlineMarkList>(input.GetHashCode());
            if(res is not null)
            {
                CacheReadCount++;
                MarkCacheSavedScanChar += input.Length;
                return res;
            }
            return null;
        }
        #endregion

        #region inclusiveCache=========

        public int ParsedSavedScanChar { get;private set; }
        public void SaveParseResult(string input, string output,List<IRule> usedRules)
        {
            if (string.IsNullOrEmpty(input))
                return;
            var cache = new InclusiveCacheValue(output, usedRules);
            _inclusiveCache.Set<InclusiveCacheValue>(input.GetHashCode(), cache, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(_options.SlideExpirationMins),
            });
        }
        public InclusiveCacheValue? ReadParseResult(string input)
        {
            var cache = _inclusiveCache.Get<InclusiveCacheValue>(input.GetHashCode());
            if (cache is not null)
            {
                CacheReadCount++;
                ParsedSavedScanChar += input.Length;
            }
            return cache;
        }

        public class InclusiveCacheValue
        {
            public string Content { get; }
            public List<IRule> UsedRules { get; }
            public InclusiveCacheValue(string content, List<IRule> usedRules)
            {
                Content = content;
                UsedRules = usedRules;
            }
        }
        #endregion

        public string DebugInfo()
        {
            string info = "";
            if (_options.UseInclusiveCache)
            {
                info += $"缓存被读取{CacheReadCount}次，避免{ParsedSavedScanChar}字符被重新解析";
            }
            else if (_options.UseExclusiveCache)
            {
                info += $"缓存被读取{CacheReadCount}次，避免{MarkCacheSavedScanChar+NonMarkSavedScanChar}字符被重新标记行内规则，避免{NonTemplateSavedScanChar}字符被重新检查模板调用";
            }
            else
            {
                info += "未开启缓存";
            }
            return HtmlLabel.DebugInfo(info);
        }
    }
}
