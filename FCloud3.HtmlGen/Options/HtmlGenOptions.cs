using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class HtmlGenOptions
    {
        public List<HtmlTemplate> Templates { get; }
        public ImplantsHandleOptions ImplantsHandleOptions { get; }
        public AutoReplaceOptions AutoReplaceOptions { get; }
        public List<IHtmlInlineRule> InlineRules { get; }
        public List<IHtmlBlockRule> BlockRules { get;}

        public HtmlGenOptions(
            ImplantsHandleOptions implantsHandleOptions,
            AutoReplaceOptions autoReplaceOptions)
        {
            ImplantsHandleOptions = implantsHandleOptions;
            AutoReplaceOptions = autoReplaceOptions;
            InlineRules = new();
            BlockRules = new();
            Templates = new();
            UsedRulesLog = new();
        }

        public List<IHtmlRule> UsedRulesLog { get; }
        public void ReportUsage(IHtmlRule rule)
        {
            if(!UsedRulesLog.Contains(rule))
                UsedRulesLog.Add(rule);
            if(rule.IsSingleUse)
            {
                if(rule is IHtmlInlineRule i)
                    InlineRules.Remove(i);
                if (rule is IHtmlBlockRule b)
                    BlockRules.Remove(b);
            }
        }
    }

    public interface IHtmlGenOptionsProvider
    {
        public HtmlGenOptions GetOptions();
    }
    public class HtmlGenOptionsBuilder : IHtmlGenOptionsProvider
    {
        private readonly HtmlGenOptions _options;
        public HtmlGenOptionsBuilder(
            List<HtmlTemplate> templates,
            List<HtmlCustomInlineRule> customInlineRules,
            List<HtmlPrefixBlockRule> customBlockRules,
            AutoReplaceOptions? autoReplaceOptions = null,
            ImplantsHandleOptions? implantsHandleOptions = null)
        {
            _options = new(implantsHandleOptions??new(),autoReplaceOptions??new());

            _options.BlockRules.AddRange(InternalBlockRules.GetInstances().Except(customBlockRules));
            _options.InlineRules.AddRange(InternalInlineRules.GetInstances().Except(customInlineRules));

            _options.Templates.AddRange(templates);
            _options.InlineRules.AddRange(customInlineRules);
            _options.BlockRules.AddRange(customBlockRules);

            if(autoReplaceOptions is not null)
            {
                var detects = autoReplaceOptions.Detects;
                detects.RemoveAll(x => x.Length < 2);
                detects.Sort((x, y) => y.Length - x.Length);
                var rules = detects.ConvertAll(x =>
                    new HtmlLiteralInlineRule(x, () => autoReplaceOptions.Replace(x)));
                _options.InlineRules.InsertRange
                (
                    index:0,
                    collection:rules
                ) ;
            }

            _options.InlineRules.Sort((x, y) => y.MarkLeft.Length - x.MarkRight.Length);
        }
        public HtmlGenOptions GetOptions()
        {
            return _options;
        }
    }
}
