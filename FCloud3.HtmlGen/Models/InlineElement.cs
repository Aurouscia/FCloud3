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
    public class FootNoteEntryElement : TextElement
    {
        public string Name { get; }
        public FootNoteEntryElement(string name):base(name) 
        {
            Name = name;
        }
        public override string ToHtml()
        {
            return $"<sup><a id=\"refentry_{Name}\" class=\"refentry\">[{Name}]</a></sup>";
        }
    }
    public sealed class CachedElement : InlineElement
    {
        public string Content { get; }
        public List<IRule>? UsedRules { get; }
        public List<IHtmlable>? FootNotes { get; }

        public CachedElement(string content, List<IRule>? usedRules, List<IHtmlable>? footNotes)
        {
            Content = content;
            UsedRules = usedRules;
            FootNotes = footNotes;
        }

        public override string ToHtml()
        {
            return Content;
        }
        public override List<IRule>? ContainRules()
        {
            return UsedRules;
        }
        public override List<IHtmlable>? ContainFootNotes()
        {
            return FootNotes;
        }
    }
}
