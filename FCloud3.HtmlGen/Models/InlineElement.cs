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
