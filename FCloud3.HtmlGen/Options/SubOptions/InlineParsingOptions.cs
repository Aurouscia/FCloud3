using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class InlineParsingOptions : IHtmlGenOptions
    {
        public List<IHtmlInlineRule> InlineRules { get; }
        public InlineParsingOptions(List<IHtmlInlineRule>? inlineRules = null)
        {
            InlineRules = inlineRules ?? new();
            InlineRules.Sort((x, y) => y.MarkLeft.Length - x.MarkRight.Length);
        }

        public void OverrideWith(IHtmlGenOptions another)
        {
            if (another is InlineParsingOptions ipo)
            {
                InlineRules.RemoveAll(ipo.InlineRules.Contains);
                InlineRules.AddRange(ipo.InlineRules);
            }
        }
    }
}
