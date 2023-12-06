using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    /// <summary>
    /// Html生成器上下文，在单次解析中唯一
    /// </summary>
    public class ParserContext
    {
        public ParserOptions Options { get; }
        public Dictionary<IRule,int> UsedRulesLog { get; }
        public TemplateSlotInfo TemplateSlotInfo { get; }
        public HashSet<int> PureStrsHash { get; }

        public int UniqueSlotIncre { get; set; }
        public int CacheReadCount { get; set; }
        public ParserContext(ParserOptions options)
        {
            UsedRulesLog = new();
            TemplateSlotInfo = new();
            Options = options;
            PureStrsHash = new();
        }
        public void ReportUsage(IRule rule)
        {
            UsedRulesLog.TryAdd(rule, 0);
            UsedRulesLog[rule] += 1;
            if (rule.IsSingleUse)
            {
                //如果是一次性的规则，使用后会被移除（不确定这样实现合不合适）
                if (rule is IHtmlInlineRule i)
                    Options.InlineParsingOptions.InlineRules.Remove(i);
                if (rule is IBlockRule b)
                    Options.BlockParsingOptions.BlockRules.Remove(b);
            }
        }
        public void ReportPureString(string str)
        {
            PureStrsHash.Add(str.GetHashCode());
        }
        public bool IsPureString(string str)
        {
            return PureStrsHash.Contains(str.GetHashCode());
        }
        public List<IRule> GetUsedRules()
        {
            return UsedRulesLog.Keys.ToList();
        }
        public int RuleUsedTime(IRule rule)
        {
            _ = UsedRulesLog.TryGetValue(rule, out int times);
            return times;
        }
    }
}
