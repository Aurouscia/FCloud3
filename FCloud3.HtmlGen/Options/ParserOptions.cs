using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
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
        public ParserOptions(TemplateParsingOptions template, ImplantsHandleOptions implant,
            AutoReplaceOptions autoReplace, InlineParsingOptions inline,BlockParsingOptions block, CacheOptions cacheOptions)
        {
            TemplateParsingOptions = template;
            ImplantsHandleOptions = implant;
            AutoReplaceOptions = autoReplace;
            InlineParsingOptions = inline;
            BlockParsingOptions = block;
            CacheOptions = cacheOptions;
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
        }
        
        public ParserOptions GetCurrentOptions()
        {
            Inline.AddMoreRules(InlineRulesFromAutoReplace(AutoReplace));
            ParserOptions options = new(Template, Implant, AutoReplace, Inline, Block, Cache);
            return options;
        }

        public Parser BuildParser()
        {
            var options = GetCurrentOptions();
            return new Parser(options);
        }

        private static List<IHtmlInlineRule> InlineRulesFromAutoReplace(AutoReplaceOptions autoReplaceOptions)
        {
            List<IHtmlInlineRule> inlineRules = new();
            if (autoReplaceOptions is not null && autoReplaceOptions.Detects.Count > 0)
            {
                var detects = autoReplaceOptions.Detects;
                detects.RemoveAll(x => x.Length < 2);
                detects.Sort((x, y) => y.Length - x.Length);
                var rules = detects.ConvertAll(x =>
                    new LiteralInlineRule(x, () => autoReplaceOptions.Replace(x)));
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
