using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.HtmlGen.Rules
{
    /// <summary>
    /// 表示一个行内规则，用户通过某种方式在行内标记出一块呈现特殊效果的区域
    /// </summary>
    public interface IInlineRule : IRule,IEquatable<IInlineRule>
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

    public abstract class InlineRule : IInlineRule
    {
        public string Name { get; }
        public string MarkLeft { get; }
        public string MarkRight { get; }
        public string PutLeft { get; }
        public string PutRight { get; }
        public string Style { get; }
        public bool IsSingleUse { get; }

        protected InlineRule(string markLeft,string markRight,string putLeft,string putRight,string style="",string name="",bool isSingleUse = false)
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
            return Equals(obj as InlineRule);
        }
        public override int GetHashCode()
        {
            return $"{MarkLeft}{MarkRight}".GetHashCode();
        }

        public bool Equals(IInlineRule? other)
        {
            if(other is InlineRule otherRule)
                if (otherRule.MarkLeft == this.MarkLeft && otherRule.MarkRight == this.MarkRight)
                    return true;
            return false;
        }
    }
    public class CustomInlineRule : InlineRule
    {
        public CustomInlineRule(string markLeft,string markRight,string putLeft,string putRight,string name="",string style = "")
            : base(markLeft, markRight, putLeft, putRight, style, name) { }
        public override bool FulFill(string span) => true;
    }
    public class RelyInlineRule : IInlineRule
    {
        public CustomInlineRule RelyOn { get; }
        public RelyInlineRule(string markLeft, string markRight, CustomInlineRule relyOn, string name = "")
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
        public bool Equals(IInlineRule? other)
        {
            if(other is RelyInlineRule rr)
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
    public class LiteralInlineRule : InlineRule
    {
        public Func<string> GetReplacement { get; }
        public LiteralInlineRule(string target,Func<string> getReplacement) 
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
    public class FootNoteAnchorRule : InlineRule
    {
        public FootNoteAnchorRule()
            : base("[^","]","","","","脚注") { }
        public override bool FulFill(string span)
        {
            return span.Length > 0 && span.Length <= 20;
        }
        public override IHtmlable MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            return new FootNoteEntryElement(span.Trim());
        }
    }
    public class ManualAnchorRule : InlineRule
    {
        public ManualAnchorRule()
            : base("[", "]", "", "", "", "手动链接") { }

        public override bool FulFill(string span)
        {
            string trimmed = span.Trim();
            return trimmed.StartsWith("http") || trimmed.StartsWith("/");
        }

        //TODO: 有图片后缀名的话变成行内图片
        public override InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            return new AnchorElement(span.Trim(), span.Trim());
        }
    }
    public class ManualTextedAnchorRule : InlineRule
    {
        private const string partsSep = "](";
        public ManualTextedAnchorRule()
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
    public class ColorTextRule : IInlineRule
    {
        public string Name => "彩色字";
        public string MarkLeft => "#";
        public string MarkRight => "#";
        public string PutLeft => "";
        public string PutRight => "";
        public bool IsSingleUse => false;
        public bool Equals(IInlineRule? other) => other is ColorTextRule;
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
                string color = span[..sepIndex];
                string text = span[(sepIndex + 2)..];

                if (HtmlColor.TryFormalize(color, out string formalColor))
                {
                    var marksWithOffset = new InlineMarkList(marks, sepIndex + 2);
                    IHtmlable textParsed = inlineParser.SplitByMarks(text, marksWithOffset);
                    return new ColorTextElement(textParsed, formalColor,this);
                }
                else
                {
                    var marksWithOffset = new InlineMarkList(marks,-MarkLeft.Length);
                    return inlineParser.SplitByMarks($"{MarkLeft}{span}{MarkRight}", marksWithOffset);
                }
            }
            else
            {
                if (HtmlColor.TryFormalize(span, out string formalColor))
                {
                    return new ColorTextElement(formalColor,this);
                }
                else
                {
                    var marksWithOffset = new InlineMarkList(marks, -MarkLeft.Length);
                    return inlineParser.SplitByMarks($"{MarkLeft}{span}{MarkRight}", marksWithOffset);
                }
            }
        }
        public class ColorTextElement : InlineElement
        {
            public string Color { get; }
            public IHtmlable Content { get; }
            public bool HaveText { get; }
            public string ClassName => HaveText ? classNameWhenText : classNameWhenEmpty;
            public string Style => HaveText ? $"color:{Color}" : $"color:{Color};background-color:{Color}";

            public const string classNameWhenText = "coloredText";
            public const string classNameWhenEmpty = "coloredBlock";
            private readonly ColorTextRule _fromRule;
            public ColorTextElement(IHtmlable content,string color,ColorTextRule fromRule) 
            {
                Color = color;
                Content = content;
                HaveText = true;
                _fromRule = fromRule;
            }
            public ColorTextElement(string color,ColorTextRule fromRule)
            {
                Color = color;
                Content = new EmptyElement();
                HaveText = false;
                _fromRule = fromRule;
            }

            public override string ToHtml()
            {
                return $"<span class=\"{ClassName}\" style=\"{Style}\">{Content.ToHtml()}</span>";
            }
            public override List<IRule>? ContainRules()
            {
                var res = Content.ContainRules()??new();
                res.Add(_fromRule);
                return res;
            }
        }
    }

    public static class InternalInlineRules
    {
        public static List<IInlineRule> GetInstances()
        {
            var centerRule1 = new CustomInlineRule("\\ct", "\\ct", "<div class=\"center\">", "</div>", "左右居中块", ".center{text-align:center}");
            var centerRule2 = new RelyInlineRule("\\中", "\\中", centerRule1);
            var mqRule1 = new CustomInlineRule("\\mq", "\\mq", "<marquee>", "</marquee>", "滚动条");
            var mqRule2 = new RelyInlineRule("\\滚", "\\滚", mqRule1);
            var instances = new List<IInlineRule>()
            {
                new FootNoteAnchorRule(),
                new ManualAnchorRule(),
                new ManualTextedAnchorRule(),
                new CustomInlineRule("*","*","<i>","</i>","斜体"),
                new CustomInlineRule("**","**","<b>","</b>","粗体"),
                new CustomInlineRule("***","***","<u>","</u>","下划线"),
                new CustomInlineRule("****","****","<s>","</s>","删除线"),
                new CustomInlineRule("~~","~~","<s>","</s>","删除线"),

                new CustomInlineRule("\\bd","\\bd","<span class=\"bordered\">","</span>","逝者",".bordered{border:1px solid black;padding:2px}"),
                new CustomInlineRule("\\hd","\\hd","<span class=\"hoverToDisplay\">","</span>","逝者",".hoverToDisplay{color:black !important;background-color:black;}.hoverToDisplay:hover{background-color:transparent;}"),
                new ColorTextRule(),
                new CustomInlineRule("\\sub","\\sub","<sub>","</sub>","下角标"),
                new CustomInlineRule("\\sup","\\sup","<sup>","</sup>","上角标"),
                centerRule1,centerRule2,
                mqRule1,mqRule2

            };
            return instances;
        }
    }
}
