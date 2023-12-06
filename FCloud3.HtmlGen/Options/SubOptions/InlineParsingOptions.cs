using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class InlineParsingOptions
    {
        public List<IHtmlInlineRule> InlineRules { get; }
        private readonly ParserBuilder _master;
        public InlineParsingOptions(ParserBuilder master, List<IHtmlInlineRule>? inlineRules = null)
        {
            _master = master;
            InlineRules = inlineRules ?? new();
            SortRules();
        }

        public ParserBuilder AddMoreRules(List<IHtmlInlineRule> inlineRules)
        {
            InlineRules.RemoveAll(inlineRules.Contains);
            InlineRules.AddRange(inlineRules);
            SortRules();
            return _master;
        }

        public ParserBuilder AddMoreRule(IHtmlInlineRule inlineRule)
        {
            InlineRules.Remove(inlineRule);
            InlineRules.Add(inlineRule);
            SortRules();
            return _master;
        }

        private void SortRules()
        {
            InlineRules.Sort((x, y) => y.MarkLeft.Length - x.MarkRight.Length);
        }
    }
}
