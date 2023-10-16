using FCloud3.HtmlGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Models
{
    public abstract class BlockElement : Element
    {
        public abstract override string ToHtml();
    }
    public class TitledBlockElement : BlockElement
    {
        public string Title { get; set; }
        public int Level { get; set; }
        public ElementCollection Content { get; set; }
        public TitledBlockElement(string title, int level, ElementCollection content)
        {
            Title = title;
            Level = level;
            Content = content;
        }

        public override string ToHtml()
        {
            return $"<h{Level}>{Title}</h{Level}><div class=\"indent\">{Content.ToHtml()}</div>";
        }
    }
    public class TypedBlockElement : BlockElement
    {
        public HtmlTypedBlockRule? Rule { get; set; }
        public ElementCollection Content { get; set; }
        public TypedBlockElement(HtmlTypedBlockRule? rule, ElementCollection content)
        {
            Rule = rule;
            Content = content;
        }

        public override string ToHtml()
        {
            if(Rule is not null)
                return $"{Rule.PutLeft}{Content.ToHtml()}{Rule.PutRight}";
            return Content.ToHtml();
        }
    }
}
