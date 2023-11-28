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
    public interface IHtmlInlineRule : IHtmlRule,IEquatable<IHtmlInlineRule>
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
        public IHtmlable MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser);
    }

    public abstract class HtmlInlineRule : IHtmlInlineRule
    {
        public string Name { get; }
        public string MarkLeft { get; }
        public string MarkRight { get; }
        public string PutLeft { get; }
        public string PutRight { get; }
        public string Style { get; }
        public bool IsSingleUse { get; }

        protected HtmlInlineRule(string markLeft,string markRight,string putLeft,string putRight,string style="",string name="",bool isSingleUse = false)
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
        public virtual IHtmlable MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            var parsed = inlineParser.SplitByMarks(span, marks);
            return new RuledInlineElement(parsed, this);
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as HtmlInlineRule);
        }
        public override int GetHashCode()
        {
            return $"{MarkLeft}{MarkRight}".GetHashCode();
        }

        public bool Equals(IHtmlInlineRule? other)
        {
            if(other is HtmlInlineRule otherRule)
                if (otherRule.MarkLeft == this.MarkLeft && otherRule.MarkRight == this.MarkRight)
                    return true;
            return false;
        }
    }
    public class HtmlCustomInlineRule : HtmlInlineRule
    {
        public HtmlCustomInlineRule(string markLeft,string markRight,string putLeft,string putRight,string name="",string style = "")
            : base(markLeft, markRight, putLeft, putRight, style, name) { }
        public override bool FulFill(string span) => true;
    }
    public class HtmlRelyInlineRule : IHtmlInlineRule
    {
        public HtmlCustomInlineRule RelyOn { get; }
        public HtmlRelyInlineRule(string markLeft, string markRight, HtmlCustomInlineRule relyOn, string name = "")
        {
            RelyOn = relyOn;
            Name = name;
            MarkLeft = markLeft;
            MarkRight = markRight;
        }
        public string Name { get; }
        public string MarkLeft { get; }
        public string MarkRight { get; }
        public string PutLeft => RelyOn.PutLeft;
        public string PutRight => RelyOn.PutRight;
        public bool IsSingleUse => RelyOn.IsSingleUse;
        public bool Equals(IHtmlInlineRule? other)
        {
            if(other is HtmlRelyInlineRule rr)
            {
                if (rr.RelyOn.Equals(this.RelyOn))
                    return this.Name == rr.Name;
            }
            return false;
        }
        public string GetPostScripts() => RelyOn.GetPostScripts();
        public string GetPreScripts() => RelyOn.GetPreScripts();
        public string GetStyles() => RelyOn.GetStyles();
        public IHtmlable MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
            => RelyOn.MakeElementFromSpan(span, marks, inlineParser);
        public bool FulFill(string span) => RelyOn.FulFill(span);
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

        public override IHtmlable MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            string[] parts = span.Split(partsSep);
            if (parts.Length != 2)
                throw new Exception($"{Name}解析异常");
            return new AnchorElement(parts[0].Trim(), parts[1].Trim());
        }
    }

    /// <summary>
    /// 当内部包含"\@"时，表示一段指定了字体颜色的文本<b>（原作者：滨蜀，此为同一规则的重新实现）</b><br/>
    /// 例子：#red\@红色文本#，#255,0,0\@红色文本#，#FF0000\@红色文本 <br/>
    /// 会被转换为：&lt;span class="coloredText" style="color:red/rgb(255,0,0)/#FF0000"&gt;红色文本&lt;/span&gt;<br/>
    /// <br/>
    /// 当内部不含"\@"时，表示一个指定了颜色的方块（宽度和高度由字体大小决定）<b>（原作者：滨蜀，此为同一规则的重新实现）</b><br/>
    /// 例子：#red#<br/>
    /// </summary>
    public class HtmlColorTextRule : IHtmlInlineRule
    {
        public string Name => "彩色字";
        public string MarkLeft => "#";
        public string MarkRight => "#";
        public string PutLeft => "";
        public string PutRight => "";
        public bool IsSingleUse => false;
        public bool Equals(IHtmlInlineRule? other) => other is HtmlColorTextRule;
        public bool FulFill(string span) => span.Length>2;
        public string GetPostScripts() => string.Empty;
        public string GetPreScripts() => string.Empty;
        public string GetStyles() => $".{ColorTextElement.classNameWhenEmpty}{{border-radius:3px}}";
        public const string sep = "\\@";
        public IHtmlable MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            int sepIndex = span.IndexOf(sep);
            if (sepIndex != -1)
            {
                var marksWithOffset = new InlineMarkList(marks, sepIndex + 2);
                string color = span[..sepIndex];
                string text = span[(sepIndex + 2)..];
                IHtmlable textParsed = inlineParser.SplitByMarks(text,marksWithOffset);
                return new ColorTextElement(textParsed, color);
            }
            else
            {
                return new ColorTextElement(span);
            }
        }
        public class ColorTextElement : InlineElement
        {
            public string Color { get; }
            public IHtmlable Content { get; }
            public bool HaveText { get; }
            public string ClassName => HaveText ? classNameWhenText : classNameWhenEmpty;

            public const string classNameWhenText = "coloredText";
            public const string classNameWhenEmpty = "coloredBlock";
            public ColorTextElement(IHtmlable content,string color) 
            {
                Color = HtmlColor.Formalize(color.Trim());
                Content = content;
                HaveText = true;
            }
            public ColorTextElement(string color)
            {
                Color = HtmlColor.Formalize(color.Trim());
                Content = new TextElement("cbk");
                HaveText = false;
            }

            public override string ToHtml()
            {
                return $"<span class=\"{ClassName}\" style=\"color:{Color}\">{Content.ToHtml()}</span>";
            }
        }
    }

    public static class InternalInlineRules
    {
        public static List<IHtmlInlineRule> GetInstances()
        {
            var centerRule1 = new HtmlCustomInlineRule("\\ct", "\\ct", "<div class=\"center\">", "</div>", "左右居中块", ".center{text-align:center}");
            var centerRule2 = new HtmlRelyInlineRule("\\中", "\\中", centerRule1);
            var mqRule1 = new HtmlCustomInlineRule("\\mq", "\\mq", "<marquee>", "</marquee>", "滚动条");
            var mqRule2 = new HtmlRelyInlineRule("\\滚", "\\滚", mqRule1);
            var instances = new List<IHtmlInlineRule>()
            {
                new HtmlManualAnchorRule(),
                new HtmlManualTextedAnchorRule(),
                new HtmlCustomInlineRule("*","*","<i>","</i>","斜体"),
                new HtmlCustomInlineRule("**","**","<b>","</b>","粗体"),
                new HtmlCustomInlineRule("***","***","<u>","</u>","下划线"),
                new HtmlCustomInlineRule("****","****","<s>","</s>","删除线"),

                new HtmlCustomInlineRule("\\bd","\\bd","<span class=\"bordered\">","</span>","逝者",".bordered{border:1px solid black}"),
                new HtmlCustomInlineRule("\\hd","\\hd","<span class=\"hoverToDisplay\">","</span>","逝者",".hoverToDisplay{color:black !important;background-color:black;}.hoverToDisplay:hover{background-color:transparent;}"),
                new HtmlColorTextRule(),
                new HtmlCustomInlineRule("\\sub","\\sub","<sub>","</sub>","下角标"),
                new HtmlCustomInlineRule("\\sup","\\sup","<sup>","</sup>","上角标"),
                centerRule1,centerRule2,
                mqRule1,mqRule2

            };
            return instances;
        }
    }
}
