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
        public ParserFootNoteContext FootNote { get; }

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
            FootNote = new();
        }
        /// <summary>
        /// 在同一个Parser对象多次反复运行之间，将一些参数设回初始值。
        /// </summary>
        public void Reset()
        {
            UniqueSlotIncre = 0;
            RuleUsage.Reset();
            Caches.Reset();
            FootNote.Clear();
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
        public void ReportUsage(List<IRule>? rules)
        {
            rules?.ForEach(ReportUsage);
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
        private readonly IMemoryCache _cache;
        private readonly CacheOptions _options;
        public  bool IsSelfHostedCache { get; }
        public ParserCacheContext(CacheOptions options)
        {
            _options = options;
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

        public int ParsedSavedScanChar { get;private set; }
        public void SaveParseResult(string input, string output,List<IRule>? usedRules, List<IHtmlable>? footNotes)
        {
            if (string.IsNullOrEmpty(input))
                return;
            var cache = new CacheValue(output, usedRules,footNotes);
            _cache.Set<CacheValue>(input, cache, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(_options.SlideExpirationMins),
            });
        }
        public CacheValue? ReadParseResult(string input)
        {
            var cache = _cache.Get<CacheValue>(input);
            if (cache is not null)
            {
                CacheReadCount++;
                ParsedSavedScanChar += input.Length;
            }
            return cache;
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
    public class ParserFootNoteContext
    {
        public List<IHtmlable> FootNoteBodys { get; }
        public ParserFootNoteContext() 
        {
            FootNoteBodys = new();
        }
        public void Clear()
        {
            FootNoteBodys.Clear();
        }
        public void AddFootNoteBody(IHtmlable body)
        {
            FootNoteBodys.Add(body);
        }
        public void AddFootNoteBodies(IEnumerable<IHtmlable?>? bodies)
        {
            if(bodies is not null)
                foreach (var b in bodies)
                {
                    if(b is not null)
                        AddFootNoteBody(b);
                }
        }
    }
}
