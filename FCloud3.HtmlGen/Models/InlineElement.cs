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
    public class ElementCollectionWithStyle : InlineElement
    {
        public HtmlInlineRule? Rule { get; }
        public ElementCollection Children { get; }
        public ElementCollectionWithStyle(HtmlInlineRule? rule = null)
        {
            Rule = rule;
            Children = new();
        }
        public void Add(Element element)
        {
            Children.Add(element);
        }
        public void AddRange(IEnumerable<Element> elements)
        {
            Children.AddRange(elements);
        }
        public override string ToHtml()
        {
            if (Rule is null)
                return Children.ToHtml();
            return $"{Rule.PutLeft}{Children.ToHtml()}{Rule.PutRight}";
        }
    }
    public class LineElement : Element
    {
        public ElementCollection Content { get; } 
        public LineElement(ElementCollection content)
        {
            Content = content;
        }
        public override string ToHtml()
        {
            return $"<p>{Content.ToHtml()}</p>";
        }
    }
    public class TextElement:InlineElement
    {
        public string Content { get; }
        public TextElement(string content)
        {
            Content = content.Trim();
        }
        public override string ToHtml()
        {
                return Content;
        }
    }
}
