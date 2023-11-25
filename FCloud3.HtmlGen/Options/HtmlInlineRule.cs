using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    /// <summary>
    /// 表示一个行内规则，用户通过某种方式在行内标记出一块呈现特殊效果的区域
    /// </summary>
    public interface IHtmlInlineRule
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
    public class HtmlInlineRule:IHtmlInlineRule
    {
        public string Name { get; }
        public string MarkLeft { get; }
        public string MarkRight { get; }
        public string PutLeft { get; }
        public string PutRight { get; }

        public HtmlInlineRule(string markLeft,string markRight,string putLeft,string putRight,string name)
        {
            MarkLeft = markLeft;
            MarkRight = markRight;
            PutLeft = putLeft;
            PutRight = putRight;
            Name = name;
        }
        public bool FulFill(string span)
        {
            return true;
        }

        public InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            return inlineParser.SplitByMarks(span,marks,this);
        }
    }
    public class HtmlManualAnchorRule : IHtmlInlineRule
    {
        public string Name => "手动链接";
        public string MarkLeft => "[";
        public string MarkRight => "]";
        public string PutLeft => string.Empty;
        public string PutRight => string.Empty;

        public bool FulFill(string span)
        {
            string trimmed = span.Trim();
            return trimmed.StartsWith("http") || trimmed.StartsWith("/");
        }
        public InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            var res = new RuledInlineElement(this);
            var anchor = new AnchorElement(span.Trim(), span.Trim());
            res.Add(anchor);
            return res;
        }
    }
    public class HtmlManualTextedAnchorRule : IHtmlInlineRule
    {
        public string Name => "带文字的手动链接";
        public string MarkLeft => "[";
        public string MarkRight => ")";
        public string PutLeft => string.Empty;
        public string PutRight => string.Empty;
        private const string partsSep = "](";
        public bool FulFill(string span)
        {
            string trimmed = span.Trim();
            string[] parts = trimmed.Split(partsSep);
            if (parts.Length != 2)
                return false;
            return (parts[1].StartsWith("http") || parts[1].StartsWith("/"));
        }

        public InlineElement MakeElementFromSpan(string span, InlineMarkList marks, IInlineParser inlineParser)
        {
            var res = new RuledInlineElement(this);
            string[] parts = span.Split(partsSep);
            if (parts.Length != 2)
                throw new Exception($"{Name}解析异常");
            var anchor = new AnchorElement(parts[0].Trim(), parts[1].Trim());
            res.Add(anchor);
            return res;
        }
    }
}
