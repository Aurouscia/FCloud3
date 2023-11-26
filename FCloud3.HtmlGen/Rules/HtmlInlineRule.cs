using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Rules
{
    /// <summary>
    /// 表示一个行内规则，用户通过某种方式在行内标记出一块呈现特殊效果的区域
    /// </summary>
    public interface IHtmlInlineRule : IHtmlRule
    {
        public string Name { get; }
        public string MarkLeft { get; }
        public string MarkRight { get; }
        public string PutLeft { get; }
        public string PutRight { get; }
        /// <summary>
        /// 被MarkLeft和MarkRight截取出的这部分区域是否适用本规则？
        /// </summary>
        /// <param name="span">截取出的区域</param>
        /// <returns>是否适用于本规则</returns>
        public bool FulFill(string span);
        public InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser);
    }

    public abstract class HtmlInlineRule:IHtmlInlineRule
    {
        public string Name { get; }
        public string MarkLeft { get; }
        public string MarkRight { get; }
        public string PutLeft { get; }
        public string PutRight { get; }
        public string Style { get; }
        public bool IsSingleUse { get; }

        public HtmlInlineRule(string markLeft,string markRight,string putLeft,string putRight,string style="",string name="",bool isSingleUse = false)
        {
            MarkLeft = markLeft;
            MarkRight = markRight;
            PutLeft = putLeft;
            PutRight = putRight;
            Style = style;
            Name = name;
            IsSingleUse = isSingleUse;
        }
        public abstract bool FulFill(string span);
        public virtual string GetStyles() => Style;
        public virtual string GetPreScripts()=>string.Empty;
        public virtual string GetPostScripts()=>string.Empty;
        public virtual InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            return inlineParser.SplitByMarks(span, marks, this);
        }
        public override bool Equals(object? obj)
        {
            if (obj is HtmlInlineRule otherRule)
                if (otherRule.MarkLeft == this.MarkLeft && otherRule.MarkRight == this.MarkRight)
                    return true;
            return false;
        }
        public override int GetHashCode()
        {
            return $"{MarkLeft}{MarkRight}".GetHashCode();
        }
    }
    public class HtmlCustomInlineRule : HtmlInlineRule
    {
        public HtmlCustomInlineRule(string markLeft,string markRight,string putLeft,string putRight,string name="",string style = "")
            : base(markLeft, markRight, putLeft, putRight, style, name) { }
        public override bool FulFill(string span) => true;
    }
    public class HtmlLiteralInlineRule : HtmlInlineRule
    {
        public Func<string> GetReplacement { get; }
        public HtmlLiteralInlineRule(string target,Func<string> getReplacement) 
            : base(markLeft:target, markRight:"", putLeft:"", putRight:"", "", "",
                  isSingleUse:true)
        {
            GetReplacement = getReplacement;
        }
        public override InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            return new TextElement(GetReplacement());
        }
        public override bool FulFill(string span)=>true;
    }
    public class HtmlManualAnchorRule : HtmlInlineRule
    {
        public HtmlManualAnchorRule()
            : base("[", "]", "", "", "", "手动链接") { }

        public override bool FulFill(string span)
        {
            string trimmed = span.Trim();
            return trimmed.StartsWith("http") || trimmed.StartsWith("/");
        }

        public override InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            return new AnchorElement(span.Trim(), span.Trim());
        }
    }
    public class HtmlManualTextedAnchorRule : HtmlInlineRule
    {
        private const string partsSep = "](";
        public HtmlManualTextedAnchorRule()
            : base("[", ")", "", "", "", "带文字的手动链接") { }
        public override bool FulFill(string span)
        {
            string trimmed = span.Trim();
            string[] parts = trimmed.Split(partsSep);
            if (parts.Length != 2)
                return false;
            return (parts[1].StartsWith("http") || parts[1].StartsWith("/"));
        }

        public override InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            string[] parts = span.Split(partsSep);
            if (parts.Length != 2)
                throw new Exception($"{Name}解析异常");
            return new AnchorElement(parts[0].Trim(), parts[1].Trim());
        }
    }

    public static class InternalInlineRules
    {
        public static List<IHtmlInlineRule> GetInstances()
        {
            return new()
            {
                new HtmlManualAnchorRule(),
                new HtmlManualTextedAnchorRule(),
                new HtmlCustomInlineRule("*","*","<i>","</i>","斜体"),
                new HtmlCustomInlineRule("**","**","<b>","</b>","粗体"),
                new HtmlCustomInlineRule("***","***","<u>","</u>","下划线"),
                new HtmlCustomInlineRule("****","****","<s>","</s>","删除线"),
                new HtmlCustomInlineRule("\\bd","\\bd","<span class=\"bordered\">","</span>","逝者",".bordered{border:1px solid black}"),
            };
        }
    }
}
