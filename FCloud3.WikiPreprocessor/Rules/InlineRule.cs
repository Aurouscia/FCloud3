using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetColorParser;
using FCloud3.WikiPreprocessor.Context;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.WikiPreprocessor.Rules
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
        public int MaxLengthBetween { get; }
        /// <summary>
        /// 被MarkLeft和MarkRight截取出的这部分区域是否适用本规则？
        /// </summary>
        /// <param name="span">截取出的区域</param>
        /// <returns>是否适用于本规则</returns>
        public bool FulFill(string span);
        public IHtmlable MakeElementFromSpan(string span,
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context);
    }

    public abstract class InlineRule : IInlineRule
    {
        public string Name { get; }
        public string MarkLeft { get; }
        public string MarkRight { get; }
        public string PutLeft { get; }
        public string PutRight { get; }
        public string Style { get; }
        public bool IsSingleUse { get; protected set; }
        public int MaxLengthBetween { get; protected set; } = int.MaxValue;

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
        public virtual IHtmlable MakeElementFromSpan(string span, 
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
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
        public string UniqueName => $"{MarkLeft}x{MarkRight}";
    }
    public class CustomInlineRule : InlineRule
    {
        public CustomInlineRule(string markLeft, string markRight, string putLeft, string putRight,
            string name = "", string style = "", int maxLengthBetween = int.MaxValue)
            : base(markLeft, markRight, putLeft, putRight, style, name)
        {
            MaxLengthBetween = maxLengthBetween;
        }
        public override bool FulFill(string span) => true;
    }
    public class RelyInlineRule : IInlineRule
    {
        public CustomInlineRule RelyOn { get; }
        public int MaxLengthBetween { get; }
        public RelyInlineRule(string markLeft, string markRight, CustomInlineRule relyOn, string name = "")
        {
            RelyOn = relyOn;
            Name = name;
            MarkLeft = markLeft;
            MarkRight = markRight;
            MaxLengthBetween = relyOn.MaxLengthBetween;
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
        public IHtmlable MakeElementFromSpan(string span,
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
            => RelyOn.MakeElementFromSpan(span, marks, inlineParser, context);
        public bool FulFill(string span) => RelyOn.FulFill(span);
        public string UniqueName => RelyOn.Name;
    }
    public class LiteralInlineRule : InlineRule
    {
        public Func<string> GetReplacement { get; }
        public LiteralInlineRule(string target, Func<string> getReplacement, bool isSingle = true) 
            : base(markLeft:target, markRight:"", putLeft:"", putRight:"", "", "",
                  isSingleUse:true)
        {
            GetReplacement = getReplacement;
            IsSingleUse = isSingle;
        }
        public override IHtmlable MakeElementFromSpan(string span,
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
        {
            var t = new TextElement(GetReplacement());
            return new RuledInlineElement(t, this);
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
        public override IHtmlable MakeElementFromSpan(string span,
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
        {
            return new FootNoteEntryElement(span.Trim(), this);
        }
    }
    public class ManualAnchorRule : InlineRule
    {
        public ManualAnchorRule()
            : base("[", "]", "", "", "", "手动链接") { }

        public override bool FulFill(string span)
        {
            return !InlineObjectRule.FulFillInlineObj(span);
        }
        
        public override IHtmlable MakeElementFromSpan(string span,
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
        {
            var trimmedSpan = span.Trim();
            if (!UrlUtil.IsUrl(trimmedSpan))
            {
                var link = context.Options.Link.LinkItems.Find(x => x.Text == trimmedSpan || x.Url == trimmedSpan);
                if (link is not null)
                {
                    // LinkItems存在 {Text:"武汉市", Url:"/w/wuhan"}
                    // [武汉市] 或 [/w/wuhan]
                    // ↓ (转换规则可自定义)
                    // <a href="/w/wuhan">武汉市</a>
                    string replacement = context.Options.Link.ConvertFn(link, null);
                    return new TextElement(replacement);
                }
                else
                {
                    return new TextElement($"[{span}]");
                }
            }
            // [https://baidu.com]
            // ↓
            // <a href="https://baidu.com">https://baidu.com</a>
            return new AnchorElement(trimmedSpan, trimmedSpan, this);
        }
    }
    public class ManualTextedAnchorRule : InlineRule
    {
        private const string partsSep = "](";
        public ManualTextedAnchorRule()
            : base("[", ")", "", "", "", "带文字的手动链接") { }
        public override bool FulFill(string span)
        {
            string[] parts = span.Split(partsSep);
            if (parts.Length != 2)
                return false;
            return true;
        }

        public override IHtmlable MakeElementFromSpan(string span,
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
        {
            string[] parts = span.Split(partsSep);
            if (parts.Length != 2)
                throw new Exception($"{Name}解析异常");
            var trimmedPart1 = parts[0].Trim();
            var trimmedPart2 = parts[1].Trim();
            if (!UrlUtil.IsUrl(trimmedPart2))
            {
                var link = context.Options.Link.LinkItems.Find(x => x.Url == trimmedPart2);
                if (link is not null)
                {
                    // LinkItems存在 {Text:"武汉市", Url:"/w/wuhan"}
                    // [武汉介绍](/w/wuhan)
                    // ↓ (转换规则可自定义)
                    // <a href="/w/wuhan">武汉介绍</a>
                    string replacement = context.Options.Link.ConvertFn(link, trimmedPart1);
                    return new TextElement(replacement);
                }
                else
                {
                    //TODO：找不到的变为红链
                    return new TextElement($"[{span})");
                }
            }
            // [百度](https://baidu.com)
            // ↓
            // <a href="https://baidu.com">百度</a>
            return new AnchorElement(trimmedPart1, trimmedPart2, this);
        }
    }
    public class InlineObjectRule : InlineRule
    {
        private const string partsSep = "|";
        private const string defaultHeight = "5em";
        public InlineObjectRule() 
            : base("[", "]", "", "", "", "行内float对象", false)
        {
        }

        public override bool FulFill(string span)
        {
            return FulFillInlineObj(span);
        }
        public static bool FulFillInlineObj(string span)
        {
            if (span.Length <= 4)
                return false;
            string[] parts = span.Split(partsSep);
            string first = parts[0];
            return UrlUtil.IsUrl(first) && UrlUtil.IsObject(first);
        }

        public override IHtmlable MakeElementFromSpan(string span,
            InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
        {
            string[] parts = span.Split(partsSep);
            string url = "";
            string height = defaultHeight;
            string? command = null;
            if(parts.Length >= 1)
            {
                url = parts[0].Trim();
            }
            if (parts.Length >= 2)
            {
                height = parts[1].Trim();
                if(int.TryParse(height,out _))
                    height += "em";
            }
            if (parts.Length >= 3)
            {
                command = parts[2].Trim();
            }
            return new InlineObjectElement(url, height, command);
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
        public int MaxLengthBetween => int.MaxValue;
        public bool Equals(IInlineRule? other) => other is ColorTextRule;
        public bool FulFill(string span) => span.Length>2;
        public string GetPostScripts() => string.Empty;
        public string GetPreScripts() => string.Empty;
        public string GetStyles() => $".{ColorTextElement.classNameWhenEmpty}{{border-radius:3px}}";
        public const string sep = "\\@";
        public IHtmlable MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser, ParserContext context)
        {
            int sepIndex = span.IndexOf(sep, StringComparison.Ordinal);
            IColorParser colorParser = context.Options.ColorParser;
            if (sepIndex != -1)
            {
                string color = span[..sepIndex];
                string text = span[(sepIndex + 2)..];

                if (ConventionalHtmlColor.TryFormalize(color, colorParser, out string formalColor))
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
                if (ConventionalHtmlColor.TryFormalize(span, colorParser, out string formalColor))
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
            public string Style => HaveText ? $"color:{Color}" : $"background-color:{Color}";

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
            public override void WriteHtml(StringBuilder sb)
            {
                sb.Append("<span class=\"");
                sb.Append(ClassName);
                sb.Append("\" style=\"");
                sb.Append(Style);
                sb.Append("\">");
                Content.WriteHtml(sb);
                sb.Append("</span>");
            }
            public override List<IRule>? ContainRules()
            {
                var res = Content.ContainRules()??new();
                res.Add(_fromRule);
                return res;
            }
        }
        public string UniqueName => "彩色字";
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
                new InlineObjectRule(),
                new ManualTextedAnchorRule(),
                new ManualAnchorRule(),
                new CustomInlineRule("****","****","<s>","</s>","删除线（不推荐）"),
                new CustomInlineRule("***","***","<u>","</u>","下划线（不推荐）"),
                new CustomInlineRule("**","**","<b>","</b>","粗体"),
                new CustomInlineRule("*","*","<i>","</i>","斜体"),
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
