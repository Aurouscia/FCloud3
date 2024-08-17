using FCloud3.WikiPreprocessor.Context.SubContext;
using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Util;
using System.Diagnostics;

namespace FCloud3.WikiPreprocessor.Context
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
        public AutoReplaceContext AutoReplace { get; }

        /// <summary>
        /// 用于模板中需要产生唯一标识符处，使用一次自增一次
        /// </summary>
        public int UniqueSlotIncre { get; set; }
        public ParserContext(ParserOptions options)
        {
            Options = options;
            RuleUsage = new();
            TemplateSlotInfo = new();
            Caches = new(options.CacheOptions, this);
            FootNote = new();
            TitleGathering = new();
            AutoReplace = new(options.InlineParsingOptions, options.AutoReplaceOptions, RuleUsage);
        }

        private int initialFrameCount = 0;
        private int frameOffsetMax = 20;
        public void SetInitialFrameCount()
        {
            initialFrameCount = new StackTrace(false).FrameCount;
        }
        public IHtmlable? FrameCountCheck()
        {
            var above = new StackTrace(initialFrameCount, false).FrameCount;
            if (above > frameOffsetMax)
                return new ErrorElement("规则嵌套层数过多");
            return null;
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
