using FCloud3.HtmlGen.Context.SubContext;
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
        public IInlineRule? Rule { get; }
        public IHtmlable Content { get; }
        public RuledInlineElement(IInlineRule? rule = null)
        {
            Rule = rule;
            Content = new ElementCollection();
        }
        public RuledInlineElement(IHtmlable htmlable,IInlineRule? rule = null)
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
        public override void WriteHtml(StringBuilder sb)
        {
            if (Rule is null)
                Content.WriteHtml(sb);
            else
            {
                sb.Append(Rule.PutLeft);
                Content.WriteHtml(sb);
                sb.Append(Rule.PutRight);
            }
        }
        public override List<IRule>? ContainRules()
        {
            var res = Content.ContainRules() ?? new();
            if (this.Rule is not null)
                res.Add(Rule);
            return res;
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
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append(Content);
        }
    }
    public class EmptyElement : InlineElement
    {
        public override string ToHtml() => string.Empty;
        public override void WriteHtml(StringBuilder sb) { }
    }
    public class AnchorElement : TextElement
    {
        public string Href { get; }
        public IRule Rule { get; }
        public AnchorElement(string content, string href, IRule ruleInstance) : base(content)
        {
            Href = href;
            Rule = ruleInstance;
        }
        public override string ToHtml()
        {
            return $"<a href=\"{Href}\">{Content}</a>";
        }
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append("<a href=\"");
            sb.Append(Href);
            sb.Append("\">");
            sb.Append(Content);
            sb.Append("</a>");
        }
        public override List<IRule>? ContainRules()
        {
            return new() { Rule };
        }
    }
    public class FootNoteEntryElement : TextElement
    {
        public string Name { get; }
        public IRule Rule { get; }
        public FootNoteEntryElement(string name, IRule ruleInstance):base(name) 
        {
            Name = name;
            Rule = ruleInstance;
        }
        public override string ToHtml()
        {
            return $"<sup><a id=\"refentry_{Name}\" class=\"refentry\">[{Name}]</a></sup>";
        }
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append("<sup><a id=\"refentry_");
            sb.Append(Name);
            sb.Append("\" class=\"refentry\">[");
            sb.Append(Name);
            sb.Append("]</a></sup>");
        }
        public override List<IRule>? ContainRules()
        {
            return new() { Rule }; 
        }
    }
    public sealed class CachedElement : InlineElement
    {
        public string Content { get; }
        public List<IRule>? UsedRules { get; }
        public List<IHtmlable>? FootNotes { get; }
        public List<ParserTitleTreeNode>? TitleNodes { get; }

        public CachedElement(string content, List<IRule>? usedRules, List<IHtmlable>? footNotes, List<ParserTitleTreeNode>? titleNodes)
        {
            Content = content;
            UsedRules = usedRules;
            FootNotes = footNotes;
            TitleNodes = titleNodes;
        }

        public override string ToHtml()
        {
            return Content;
        }
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append(Content);
        }
        public override List<IRule>? ContainRules()
        {
            return UsedRules;
        }
        public override List<IHtmlable>? ContainFootNotes()
        {
            return FootNotes;
        }
        public override List<ParserTitleTreeNode>? ContainTitleNodes()
        {
            return TitleNodes;
        }
    }
}
