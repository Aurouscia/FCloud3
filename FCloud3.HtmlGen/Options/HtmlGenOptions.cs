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
        public List<IHtmlInlineRule> InlineRules { get; }
        public List<IHtmlBlockRule> BlockRules { get;}

        public HtmlGenOptions()
        {
            Templates = new();
            InlineRules = new();
            BlockRules = new();
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
            List<HtmlTemplate> templates, List<HtmlInlineRule> inlineRules,List<HtmlPrefixBlockRule> blockRules)
        {
            _options = new();
            _options.Templates.AddRange(templates);
            _options.InlineRules.AddRange(inlineRules);
            _options.BlockRules.AddRange(blockRules);

            _options.InlineRules.Add(new HtmlManualAnchorRule());
            _options.InlineRules.Add(new HtmlManualTextedAnchorRule());
            _options.BlockRules.Add(new HtmlMiniTableBlockRule());

            _options.InlineRules.Sort((x, y) => y.MarkLeft.Length - x.MarkRight.Length);
        }
        public HtmlGenOptions GetOptions()
        {
            return _options;
        }
    }
}
