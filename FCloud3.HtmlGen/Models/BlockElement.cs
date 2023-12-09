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
        public override List<IRule>? ContainRules()
        {
            return Content.ContainRules();
        }
    }
    public class TitledBlockElement : BlockElement
    {
        public IHtmlable Title { get; }
        public int Level { get; }
        private readonly string? _rawLineHash;
        
        public TitledBlockElement(IHtmlable title,string? rawLineHash, int level, IHtmlable content):base(content)
        {
            Title = title;
            Level = level;
            _rawLineHash = rawLineHash;
        }

        public override string ToHtml()
        {
            string body = $"<div class=\"indent\">{Content.ToHtml()}</div>";
            if (_rawLineHash is null)
                return $"<h{Level}>{Title.ToHtml()}</h{Level}>{body}";
            else
                return $"{HtmlLabel.Custom(Title.ToHtml(), $"h{Level}", Consts.locatorAttrName, _rawLineHash)}{body}";
        }
    }
    public class RuledBlockElement: BlockElement
    {
        public IBlockRule? GenByRule { get; }
        public RuledBlockElement(IHtmlable content, IBlockRule? genByRule):base(content)
        {
            GenByRule = genByRule;
        }
        public override string ToHtml()
        {
            if(GenByRule is not null)
                return GenByRule.Apply(Content);
            return base.ToHtml();
        }
        public override List<IRule>? ContainRules()
        {
            var res = Content.ContainRules()??new();
            if (GenByRule is not null)
                res.Add(GenByRule);
            return res;
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
