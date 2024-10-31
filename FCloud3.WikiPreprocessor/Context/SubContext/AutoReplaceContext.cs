using FCloud3.WikiPreprocessor.Options.SubOptions;
using FCloud3.WikiPreprocessor.Rules;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    /// <summary>
    /// “自动替换”上下文<br/>
    /// 自动替换会被转换为字面量行内规则(LiteralInlineRule)<br/>
    /// 本类负责追踪自动替换目标与对应的LiteralInlineRule，在不再需要时将它们移除<br/>
    /// 可更换或补充自动替换目标
    /// </summary>
    public class AutoReplaceContext
    {
        private readonly InlineParsingOptions _inlineParsingOptions;
        private readonly ParserRuleUsageContext _ruleUsageContext;
        private readonly ParserContext _ctx;
        public AutoReplaceContext(
            InlineParsingOptions inlineOptions,
            AutoReplaceOptions autoReplaceOptions,
            ParserRuleUsageContext ruleUsageContext,
            ParserContext ctx)
        {
            _inlineParsingOptions = inlineOptions;
            _ruleUsageContext = ruleUsageContext;
            _ctx = ctx;
            Register(autoReplaceOptions.Detects.ToList());//初次传入的替换目标立即注册
        }
        public List<(ReplaceTarget target, LiteralInlineRule rule)> ActiveReplaces = [];
        
        public void Register(List<string?> targets, bool isSingleUse = true, bool clearOld = true)
        {
            targets.RemoveAll(x => x is null);
            List<ReplaceTarget> ts = targets.ConvertAll(x => new ReplaceTarget(x!, isSingleUse));
            Register(ts, clearOld);
        }
        public void Register(List<ReplaceTarget> targets, bool clearOld = true)
        {
            if(clearOld)
                Clear();
            var rules = InlineRulesFromAutoReplace(targets);
            ActiveReplaces.AddRange(rules);
            rules.ForEach(x =>
            {
                _inlineParsingOptions.InlineRules.Insert(0, x.rule); 
            });
        }
        /// <summary>
        /// 从行内规则集合和规则使用记录中去除旧版的替换，确保旧的替换不影响新的替换
        /// 如果规则使用记录仍有旧版的记录，若新版中有相同替换目标（且一次性），就不会生效，所以必须去除旧版的使用记录
        /// </summary>
        public void Clear()
        {
            ActiveReplaces.ForEach(x =>
            {
                _ruleUsageContext.UsedRulesLog.Remove(x.rule);
                _inlineParsingOptions.InlineRules.Remove(x.rule);
            });
            ActiveReplaces.Clear();
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
                        getReplacement: () => _ctx.DataSource?.Replace(x.Text),
                        isSingle: x.IsSingleUse
                    );
                    res.Add((x, rule));
                });
            }
            return res;
        }
    }
}