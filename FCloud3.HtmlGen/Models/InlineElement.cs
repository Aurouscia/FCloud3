using FCloud3.HtmlGen.Options;
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
        public ElementCollection Content { get; }
        public RuledInlineElement(IHtmlInlineRule? rule = null)
        {
            Rule = rule;
            Content = new();
        }
        public void Add(Element element)
        {
            Content.Add(element);
        }
        public void AddRange(IEnumerable<Element> elements)
        {
            Content.AddRange(elements);
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
            Content = content;
        }
        public override string ToHtml()
        {
            return Content;
        }
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
