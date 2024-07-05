using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Options.SubOptions;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Util;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using DotNetColorParser;
using DotNetColorParser.ColorNotations;

namespace FCloud3.WikiPreprocessor.Options
{
    /// <summary>
    /// Html生成器的启动选项类
    /// </summary>
    public class ParserOptions
    {
        public TemplateParsingOptions TemplateParsingOptions { get; }
        public ImplantsHandleOptions ImplantsHandleOptions { get; }
        public AutoReplaceOptions AutoReplaceOptions { get; }
        public InlineParsingOptions InlineParsingOptions { get; }
        public BlockParsingOptions BlockParsingOptions { get;}
        public CacheOptions CacheOptions { get; }
        public TitleGatheringOptions TitleGatheringOptions { get; }
        public LinkOptions Link { get; }
        public bool Debug { get; }
        public bool ClearRuleUsageOnCall { get; }
        public IColorParser ColorParser { get; }
        public ILocatorHash? LocatorHash { get; }
        public ParserOptions(
            TemplateParsingOptions template,
            ImplantsHandleOptions implant, 
            AutoReplaceOptions autoReplace, 
            InlineParsingOptions inline,
            BlockParsingOptions block,
            CacheOptions cacheOptions,
            TitleGatheringOptions titleGathering,
            LinkOptions link,
            bool debug, bool clearRuleUsageOnCall,
            IColorParser colorParser, ILocatorHash? locatorHash)
        {
            TemplateParsingOptions = template;
            ImplantsHandleOptions = implant;
            AutoReplaceOptions = autoReplace;
            InlineParsingOptions = inline;
            BlockParsingOptions = block;
            CacheOptions = cacheOptions;
            TitleGatheringOptions = titleGathering;
            Link = link;
            Debug = debug;
            ClearRuleUsageOnCall = clearRuleUsageOnCall;
            ColorParser = colorParser;
            LocatorHash = locatorHash;
        }
    }

    public class ParserBuilder
    {
        public TemplateParsingOptions Template { get; }
        public ImplantsHandleOptions Implant { get; }
        public AutoReplaceOptions AutoReplace { get; }
        public InlineParsingOptions Inline { get; }
        public BlockParsingOptions Block { get; }
        public CacheOptions Cache { get; }
        public TitleGatheringOptions TitleGathering { get; }
        public LinkOptions Link { get; }
        public bool Debug { get; private set; }
        public bool ClearRuleUsageOnCall { get; private set; }
        public IColorParser? ColorParser { get; private set; }
        public ILocatorHash? LocatorHash { get; private set; }
        public ParserBuilder()
        {
            Template = new(this);
            Implant = new(this);
            AutoReplace = new(this);
            Inline = new(this);
            Block = new(this);
            Cache = new(this);
            TitleGathering = new(this);
            Link = new(this);
            
            Block.AddMoreRules(InternalBlockRules.GetInstances());
            Inline.AddMoreRules(InternalInlineRules.GetInstances());
            Template.AddTemplates(InternalTemplates.GetInstances());
        }
        
        public ParserBuilder EnableDebugInfo()
        {
            Debug = true; return this;
        }
        public ParserBuilder UseLocatorHash(ILocatorHash locatorHash)
        {
            LocatorHash = locatorHash;
            return this;
        }
        public ParserBuilder ClearUsageInfoOnCall()
        {
            ClearRuleUsageOnCall = true;
            return this;
        }
        public ParserBuilder UseColorParser(IColorParser colorParser)
        {
            ColorParser = colorParser;
            return this;
        } 

        public ParserOptions GetCurrentOptions()
        {
            Inline.AddMoreRules(InlineRulesFromAutoReplace(AutoReplace));
            ColorParser ??= FallToColorParser();
            ParserOptions options = 
                new(Template, Implant, AutoReplace, Inline, Block, Cache, TitleGathering, Link,
                Debug, ClearRuleUsageOnCall, ColorParser, LocatorHash);
            return options;
        }

        public Parser BuildParser()
        {
            var options = GetCurrentOptions();
            return new Parser(options);
        }

        /// <summary>
        /// AutoReplace会被转译为字面量行内规则(LiteralInlineRules)被行内解析器(InlineParser)匹配
        /// </summary>
        /// <param name="autoReplaceOptions"></param>
        /// <returns></returns>
        private static List<IInlineRule> InlineRulesFromAutoReplace(AutoReplaceOptions autoReplaceOptions)
        {
            List<IInlineRule> inlineRules = new();
            if (autoReplaceOptions.Detects.Count > 0)
            {
                var detects = autoReplaceOptions.Detects;
                detects.RemoveAll(x => x.Text.Length < 2);
                detects.Sort((x, y) => y.Text.Length - x.Text.Length);
                var rules = detects.ConvertAll(x =>
                    new LiteralInlineRule(
                        target: x.Text,
                        getReplacement: () => autoReplaceOptions.Replace(x.Text),
                        isSingle: x.IsSingleUse
                    ));
                inlineRules.InsertRange
                (
                    index: 0,
                    collection: rules
                );
            }
            return inlineRules;
        }

        private static IColorParser FallToColorParser()
        {
            ColorNotationProvider colorNotationProvider = [ 
                new KnownColorNameNotation(), new HexRGBANotation(), new RGBNotation(), new HSLNotation()];
            return new ColorParser(colorNotationProvider);
        }
    }
}
