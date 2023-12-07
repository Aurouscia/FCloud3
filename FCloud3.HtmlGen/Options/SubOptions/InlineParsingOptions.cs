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
        public List<IInlineRule> InlineRules { get; }
        private readonly ParserBuilder _master;
        public InlineParsingOptions(ParserBuilder master, List<IInlineRule>? inlineRules = null)
        {
            _master = master;
            InlineRules = inlineRules ?? new();
            SortRules();
        }

        public ParserBuilder AddMoreRules(List<IInlineRule> inlineRules)
        {
            InlineRules.RemoveAll(inlineRules.Contains);
            InlineRules.AddRange(inlineRules);
            SortRules();
            return _master;
        }

        public ParserBuilder AddMoreRule(IInlineRule inlineRule)
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
