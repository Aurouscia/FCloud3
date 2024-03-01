using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
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
        public bool Debug { get; }
        public ILocatorHash? LocatorHash { get; }
        public ParserOptions(TemplateParsingOptions template, ImplantsHandleOptions implant, AutoReplaceOptions autoReplace, 
            InlineParsingOptions inline,BlockParsingOptions block, CacheOptions cacheOptions, bool debug, ILocatorHash? locatorHash)
        {
            TemplateParsingOptions = template;
            ImplantsHandleOptions = implant;
            AutoReplaceOptions = autoReplace;
            InlineParsingOptions = inline;
            BlockParsingOptions = block;
            CacheOptions = cacheOptions;
            Debug = debug;
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
        public bool Debug { get; private set; }
        public ILocatorHash? LocatorHash { get; private set; }
        public ParserBuilder()
        {
            Template = new(this);
            Implant = new(this);
            AutoReplace = new(this);
            Inline = new(this);
            Block = new(this);
            Cache = new(this);

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
            LocatorHash = locatorHash;return this;
        }

        public ParserOptions GetCurrentOptions()
        {
            Inline.AddMoreRules(InlineRulesFromAutoReplace(AutoReplace));
            ParserOptions options = new(Template, Implant, AutoReplace, Inline, Block, Cache, Debug, LocatorHash);
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
            if (autoReplaceOptions is not null && autoReplaceOptions.Detects.Count > 0)
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
    }
}
