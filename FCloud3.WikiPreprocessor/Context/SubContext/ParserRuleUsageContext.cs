﻿using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Util;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    /// <summary>
    /// “规则使用”上下文<br/>
    /// 用于记录各个规则使用的次数<br/>
    /// 会被用来判断“一次性”规则是否已经被用过（有记录）
    /// </summary>
    public class ParserRuleUsageContext
    {
        public Dictionary<IRule, int> UsedRulesLog { get; }

        public ParserRuleUsageContext()
        {
            UsedRulesLog = new();
        }
        public void ReportUsage(IRule rule)
        {
            UsedRulesLog.TryAdd(rule, 0);
            UsedRulesLog[rule] += 1;
        }
        public void ReportUsage(List<IRule>? rules)
        {
            rules?.ForEach(ReportUsage);
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
        public bool ViolateSingleUsage(List<IRule> usingRules)
        {
            var usingSingles = usingRules.FindAll(x => x.IsSingleUse);
            if (usingSingles.Count() == 0) return false;
            var usedSingles = UsedRulesLog.Keys.Where(x => x.IsSingleUse);
            return usedSingles.Intersect(usingSingles).Any();
        }
        public string DebugInfo()
        {
            int rulesCount = UsedRulesLog.Count;
            int totalUsage = UsedRulesLog.Values.Sum();
            string info = $"共使用{rulesCount}种规则，共{totalUsage}处";
            return HtmlLabel.DebugInfo(info);
        }
        public void Reset()
        {
            UsedRulesLog.Clear();
        }
    }
}
