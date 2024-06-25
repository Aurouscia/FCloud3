using FCloud3.WikiPreprocessor.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class InlineParsingOptions
    {
        public List<IInlineRule> InlineRules { get; }
        private readonly ParserBuilder _master;
        public InlineParsingOptions(ParserBuilder master, List<IInlineRule>? inlineRules = null)
        {
            _master = master;
            InlineRules = inlineRules ?? new();
        }

        public ParserBuilder AddMoreRules(List<IInlineRule> inlineRules)
        {
            InlineRules.RemoveAll(inlineRules.Contains);
            InlineRules.AddRange(inlineRules);
            return _master;
        }

        public ParserBuilder AddMoreRule(IInlineRule inlineRule)
        {
            InlineRules.Remove(inlineRule);
            InlineRules.Add(inlineRule);
            return _master;
        }
    }
}
