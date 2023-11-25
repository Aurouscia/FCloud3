using FCloud3.HtmlGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Models
{
    public class BlockElement : Element
    {
        public ElementCollection Content { get; }
        public BlockElement(ElementCollection content)
        {
            Content = content;
        }

        public override string ToHtml()
        {
            return Content.ToHtml();
        }
    }
    public class TitledBlockElement : BlockElement
    {
        public string Title { get; }
        public int Level { get; }
        
        public TitledBlockElement(string title, int level, ElementCollection content):base(content)
        {
            Title = title;
            Level = level;
        }

        public override string ToHtml()
        {
            return $"<h{Level}>{Title}</h{Level}><div class=\"indent\">{Content.ToHtml()}</div>";
        }
    }
    public class RuledBlockElement: BlockElement
    {
        public IHtmlBlockRule? GenByRule { get; }
        public RuledBlockElement(ElementCollection content, IHtmlBlockRule? genByRule):base(content)
        {
            GenByRule = genByRule;
        }
        public override string ToHtml()
        {
            if(GenByRule is not null)
                return GenByRule.Apply(Content);
            return base.ToHtml();
        }
    }
    public class SimpleBlockElement : BlockElement
    {
        public string PutLeft { get; }
        public string PutRight { get; }
        public SimpleBlockElement(ElementCollection content, string putLeft, string putRight):base(content)
        {
            PutLeft = putLeft;
            PutRight = putRight;
        }
        public override string ToHtml()
        {
            return $"{PutLeft}{base.ToHtml()}{PutRight}";
        }
    }
}
