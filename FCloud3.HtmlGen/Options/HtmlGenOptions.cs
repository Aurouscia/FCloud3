using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class HtmlGenOptions
    {
        public List<HtmlTemplate> Templates { get; }
        public Func<string,string?> Implants { get; set; }
        public List<IHtmlInlineRule> InlineRules { get; }
        public List<IHtmlBlockRule> BlockRules { get;}

        public HtmlGenOptions()
        {
            Templates = new();
            Implants = x => null;
            InlineRules = new();
            BlockRules = new();
            UsedRulesLog = new();
        }

        public List<IHtmlRule> UsedRulesLog { get; }
        public void ReportUsage(IHtmlRule rule)
        {
            if(!UsedRulesLog.Contains(rule))
                UsedRulesLog.Add(rule);
        }
    }

    public interface IHtmlGenOptionsProvider
    {
        public HtmlGenOptions GetOptions();
    }
    public class HtmlGenOptionsProvider : IHtmlGenOptionsProvider
    {
        private readonly HtmlGenOptions _options;
        public HtmlGenOptionsProvider(
            List<HtmlTemplate> templates,
            List<HtmlCustomInlineRule> customInlineRules,
            List<HtmlPrefixBlockRule> customBlockRules,
            Func<string,string?> implantsHandler)
        {
            _options = new();

            _options.BlockRules.AddRange(InternalBlockRules.GetInstances().Except(customBlockRules));
            _options.InlineRules.AddRange(InternalInlineRules.GetInstances().Except(customInlineRules));

            _options.Templates.AddRange(templates);
            _options.Implants = implantsHandler;
            _options.InlineRules.AddRange(customInlineRules);
            _options.BlockRules.AddRange(customBlockRules);

            _options.InlineRules.Sort((x, y) => y.MarkLeft.Length - x.MarkRight.Length);
        }
        public HtmlGenOptions GetOptions()
        {
            return _options;
        }
    }
}
