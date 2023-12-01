using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
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
    public class HtmlGenContext
    {
        public HtmlGenOptions Options { get; }
        public Dictionary<IHtmlRule,int> UsedRulesLog { get; }
        public TemplateSlotInfo TemplateSlotInfo { get; }
        public int UniqueSlotIncre { get; set; }
        public HtmlGenContext(HtmlGenOptions options)
        {
            UsedRulesLog = new();
            TemplateSlotInfo = new();
            Options = options;
        }
        public void ReportUsage(IHtmlRule rule)
        {
            UsedRulesLog.TryAdd(rule, 0);
            UsedRulesLog[rule] += 1;
            if (rule.IsSingleUse)
            {
                //如果是一次性的规则，使用后会被移除（不确定这样实现合不合适）
                if (rule is IHtmlInlineRule i)
                    Options.InlineParsingOptions.InlineRules.Remove(i);
                if (rule is IHtmlBlockRule b)
                    Options.BlockParsingOptions.BlockRules.Remove(b);
            }
        }
        public List<IHtmlRule> GetUsedRules()
        {
            return UsedRulesLog.Keys.ToList();
        }
        public int RuleUsedTime(IHtmlRule rule)
        {
            _ = UsedRulesLog.TryGetValue(rule, out int times);
            return times;
        }
    }
}
