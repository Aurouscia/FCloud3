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
        public override void WriteHtml(StringBuilder sb)
        {
            Content.WriteHtml(sb);
        }
        public override List<IRule>? ContainRules()
        {
            return Content.ContainRules();
        }
        public override List<IHtmlable>? ContainFootNotes()
        {
            return Content.ContainFootNotes();
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
        public override void WriteHtml(StringBuilder sb)
        {
            if (_rawLineHash is null)
            {
                sb.Append("<h");sb.Append(Level);sb.Append('>');
            }
            else
            {
                sb.Append("<h"); sb.Append(Level);
                sb.Append(' '); sb.Append(Consts.locatorAttrName);
                sb.Append("=\""); sb.Append(_rawLineHash);
                sb.Append("\">");
            }
            Title.WriteHtml(sb);
            sb.Append("</h"); sb.Append(Level); sb.Append('>');
            sb.Append("<div class=\"indent\">");
            Content.WriteHtml(sb);
            sb.Append("</div>");
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
        public override void WriteHtml(StringBuilder sb)
        {
            if (GenByRule is not null)
                GenByRule.ApplyWrite(sb, Content);
            else
                base.WriteHtml(sb);
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
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append(PutLeft);
            base.WriteHtml(sb);
            sb.Append(PutRight);
        }
    }
    public class FootNoteBodyElement : BlockElement
    {
        public string Name { get; }
        public string? Hash { get; }
        public FootNoteBodyElement(string name, IHtmlable content, string? hash) :base(content)
        {
            Name = name;
            Hash = hash;
        }
        public override string ToHtml()
        {
            string main = $"<a id=\"ref_{Name}\" class=\"ref\">[{Name}]</a>{base.ToHtml()}";
            if(Hash is not null)
                return $"<div loc=\"{Hash}\">{main}</div>";
            else
                return $"<div>{main}</div>";
        }
        public override void WriteHtml(StringBuilder sb)
        {
            if (Hash is not null)
            {
                sb.Append("<div loc=\"");
                sb.Append(Hash);
                sb.Append("\">");
            }
            else
                sb.Append("<div>");
            sb.Append($"<a id=\"ref_{Name}\" class=\"ref\">[{Name}]</a>");
            base.WriteHtml(sb);
            sb.Append("</div>");
        }
    }
    public class FootNoteBodyPlaceholderElement:BlockElement
    {
        public FootNoteBodyElement Body { get; }
        public FootNoteBodyPlaceholderElement(FootNoteBodyElement body)
        {
            Body = body;
        }
        public override string ToHtml() => string.Empty;
        public override List<IHtmlable>? ContainFootNotes()
        {
            return new() { Body };
        }
    }
}
