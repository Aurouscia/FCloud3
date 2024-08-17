using FCloud3.WikiPreprocessor.Options.SubOptions;
using FCloud3.WikiPreprocessor.Rules;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    public class AutoReplaceContext
    {
        private readonly InlineParsingOptions _inlineParsingOptions;
        private readonly AutoReplaceOptions _autoReplaceOptions;
        public AutoReplaceContext(
            InlineParsingOptions inlineOptions,
            AutoReplaceOptions autoReplaceOptions)
        {
            _inlineParsingOptions = inlineOptions;
            _autoReplaceOptions = autoReplaceOptions;
            Register(autoReplaceOptions.Detects);//初次传入的替换目标立即注册
        }
        public List<(ReplaceTarget target, LiteralInlineRule rule)> ActiveReplaces = [];

        public void Register(List<string> targets, bool isSingleUse = true, bool clear = true)
        {
            List<ReplaceTarget> ts = targets.ConvertAll(x => new ReplaceTarget(x, isSingleUse));
            Register(ts, clear);
        }
        public void Register(List<ReplaceTarget> targets, bool clear = true)
        {
            if(clear)
                Clear();
            var rules = InlineRulesFromAutoReplace(targets);
            ActiveReplaces.AddRange(rules);
            rules.ForEach(x =>
            {
                _inlineParsingOptions.InlineRules.Insert(0, x.rule); 
            });
        }
        public void Clear()
        {
            ActiveReplaces.ForEach(x =>
            {
                _inlineParsingOptions.InlineRules.Remove(x.rule);
            });
        }
        
        /// <summary>
        /// AutoReplace会被转译为字面量行内规则(LiteralInlineRules)被行内解析器(InlineParser)匹配
        /// </summary>
        /// <param name="targets"></param>
        /// <returns></returns>
        private List<(ReplaceTarget target, LiteralInlineRule rule)> InlineRulesFromAutoReplace(List<ReplaceTarget> targets)
        {
            List<(ReplaceTarget target, LiteralInlineRule rule)> res = new();
            if (targets.Count > 0)
            {
                targets.RemoveAll(x => x.Text.Length < 2);
                targets.Sort((x, y) => x.Text.Length - y.Text.Length);
                targets.ForEach(x =>
                {
                    var rule = new LiteralInlineRule(
                        target: x.Text,
                        getReplacement: () => _autoReplaceOptions.Replace(x.Text),
                        isSingle: x.IsSingleUse
                    );
                    res.Add((x, rule));
                });
            }
            return res;
        }
    }
}