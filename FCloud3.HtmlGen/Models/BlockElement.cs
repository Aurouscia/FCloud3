using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Models
{
    public abstract class BlockElement : Element
    {
        public IHtmlable Content { get; }
        public BlockElement(IHtmlable? content = null)
        {
            Content = content ?? new EmptyElement();
        }

        public override string ToHtml()
        {
            return Content.ToHtml();
        }
    }
    public class TitledBlockElement : BlockElement
    {
        public IHtmlable Title { get; }
        public int Level { get; }
        
        public TitledBlockElement(IHtmlable title, int level, IHtmlable content):base(content)
        {
            Title = title;
            Level = level;
        }

        public override string ToHtml()
        {
            return $"<h{Level}>{Title.ToHtml()}</h{Level}><div class=\"indent\">{Content.ToHtml()}</div>";
        }
    }
    public class RuledBlockElement: BlockElement
    {
        public IHtmlBlockRule? GenByRule { get; }
        public RuledBlockElement(IHtmlable content, IHtmlBlockRule? genByRule):base(content)
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
        public SimpleBlockElement(IHtmlable content, string putLeft, string putRight):base(content)
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
