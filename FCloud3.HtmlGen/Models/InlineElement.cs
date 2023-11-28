using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Models
{
    public abstract class InlineElement:Element
    {
    }
    public class RuledInlineElement : InlineElement
    {
        public IHtmlInlineRule? Rule { get; }
        public IHtmlable Content { get; }
        public RuledInlineElement(IHtmlInlineRule? rule = null)
        {
            Rule = rule;
            Content = new ElementCollection();
        }
        public RuledInlineElement(IHtmlable htmlable,IHtmlInlineRule? rule = null)
        {
            Rule = rule;
            Content = htmlable;
        }
        public override string ToHtml()
        {
            if (Rule is null)
                return Content.ToHtml();
            return $"{Rule.PutLeft}{Content.ToHtml()}{Rule.PutRight}";
        }
    }
    public class TextElement:InlineElement
    {
        public string Content { get; }
        public TextElement(string content)
        {
            Content = Escape.HideEscapeMark(content.Trim());
        }
        public override string ToHtml()
        {
            return Content;
        }
    }
    public class EmptyElement : InlineElement
    {
        public override string ToHtml() => string.Empty;
    }
    public class AnchorElement : TextElement
    {
        public string Href { get; }
        public AnchorElement(string content, string href) : base(content)
        {
            Href = href;
        }
        public override string ToHtml()
        {
            return $"<a href=\"{Href}\">{Content}</a>";
        }
    }
}
