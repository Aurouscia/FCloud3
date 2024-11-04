using FCloud3.WikiPreprocessor.Context.SubContext;
using FCloud3.WikiPreprocessor.DataSource;
using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Util;
using System.Diagnostics;

namespace FCloud3.WikiPreprocessor.Context
{
    /// <summary>
    /// Html生成器上下文，在单次解析中唯一
    /// </summary>
    public class ParserContext
    {
        public ParserOptions Options { get; }
        public TemplateSlotInfo TemplateSlotInfo { get; }
        public ParserCacheContext Caches { get; }
        public ParserRuleUsageContext RuleUsage { get; }
        public ParserFootNoteContext FootNote { get; }
        public ParserTitleGatheringContext TitleGathering { get; }
        public ParserRefContext Ref { get; }
        public AutoReplaceContext AutoReplace { get; }
        public IScopedDataSource? DataSource { get; private set; }

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
            Ref = new();
            AutoReplace = new(
                options.InlineParsingOptions,
                options.AutoReplaceOptions,
                RuleUsage,
                this);
        }

        private int initialFrameCount = 0;
        private const int frameOffsetMax = 20;
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
        /// 在Parser对象运行之前，将一些参数设回初始值
        /// </summary>
        public void BeforeParsing()
        {
            UniqueSlotIncre = 0;
            if(!Options.KeepRuleUsage)
                RuleUsage.Reset();
            Caches.BeforeParsing();
            FootNote.Clear();
            Ref.Clear();
        }
        /// <summary>
        /// 在Parser对象运行之后，抛弃Scoped数据源，确保每次拿到的都是新的
        /// </summary>
        public void AfterParsing()
        {
            Caches.AfterParsing();
            DataSource = null;
        }
        public void ClearRuleUsage()
        {
            RuleUsage.Reset();
        }
        public void SetDataSource(IScopedDataSource dataSource)
        {
            DataSource = dataSource;
        }
        public string DebugInfo()
        {
            return Caches.DebugInfo() + RuleUsage.DebugInfo();
        }
    }
}
