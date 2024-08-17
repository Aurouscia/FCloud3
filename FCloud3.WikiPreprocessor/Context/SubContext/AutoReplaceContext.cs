using FCloud3.WikiPreprocessor.Options.SubOptions;
using FCloud3.WikiPreprocessor.Rules;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    public class AutoReplaceContext
    {
        public Dictionary<ReplaceTarget, LiteralInlineRule> ActiveReplaces = [];

        public AutoReplaceContext(InlineParsingOptions inlineOptions, AutoReplaceOptions autoReplaceOptions)
        {
            
        }
        
        public void Register(ReplaceTarget target, LiteralInlineRule rule)
        {
            ActiveReplaces.Add(target, rule);
        }
        public void Clear()
        {
            ActiveReplaces.Clear();
        }
    }
}