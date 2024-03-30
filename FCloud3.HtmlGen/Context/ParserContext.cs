using FCloud3.HtmlGen.Context.SubContext;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;

namespace FCloud3.HtmlGen.Context
{
    /// <summary>
    /// Html生成器上下文，在单次解析中唯一
    /// </summary>
    public class ParserContext : IDisposable
    {
        public ParserOptions Options { get; }
        public TemplateSlotInfo TemplateSlotInfo { get; }
        public ParserCacheContext Caches { get; }
        public ParserRuleUsageContext RuleUsage { get; }
        public ParserFootNoteContext FootNote { get; }
        public ParserTitleGatheringContext TitleGathering { get; }

        /// <summary>
        /// 用于模板中需要产生唯一标识符处，使用一次自增一次
        /// </summary>
        public int UniqueSlotIncre { get; set; }
        public ParserContext(ParserOptions options)
        {
            RuleUsage = new();
            TemplateSlotInfo = new();
            Options = options;
            Caches = new(options.CacheOptions, this);
            FootNote = new();
            TitleGathering = new();
        }
        /// <summary>
        /// 在同一个Parser对象多次反复运行之间，将一些参数设回初始值。
        /// </summary>
        public void Reset(bool enforce = false)
        {
            UniqueSlotIncre = 0;
            if( Options.ClearRuleUsageOnCall || enforce)
                RuleUsage.Reset();
            Caches.Reset();
            FootNote.Clear();
        }
        public string DebugInfo()
        {
            return Caches.DebugInfo() + RuleUsage.DebugInfo();
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
}
