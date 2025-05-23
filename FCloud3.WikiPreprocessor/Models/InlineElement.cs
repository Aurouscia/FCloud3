﻿using FCloud3.WikiPreprocessor.Context.SubContext;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Models
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
        public override void WriteBody(StringBuilder sb, int maxLength)
        {
            Content.WriteBody(sb, maxLength);
        }
    }
    public class TextElement:InlineElement
    {
        public string Content { get; }
        public TextElement(string content)
        {
            Content = Escape.HideEscapeMark(content);
        }
        public override string ToHtml()
        {
            return Content;
        }
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append(Content);
        }
        public override void WriteBody(StringBuilder sb, int maxLength)
        {
            int leftLength = maxLength - sb.Length;
            if (leftLength <= 0)
                return;
            if (Content.Length <= leftLength)
                sb.Append(Content);
            else
                sb.Append(Content.AsSpan()[..leftLength]);
        }
    }
    public class TextConvertedElement(
        string originalText, string convertedText) 
        : InlineElement
    {
        public string OriginalText { get; set; } = originalText;
        public string ConvertedText { get; set; } = convertedText;
        public override string ToHtml()
        {
            return ConvertedText;
        }
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append(ConvertedText);
        }
        public override void WriteBody(StringBuilder sb, int maxLength)
        {
            int leftLength = maxLength - sb.Length;
            if (leftLength <= 0)
                return;
            if (OriginalText.Length <= leftLength)
                sb.Append(OriginalText);
            else
                sb.Append(OriginalText.AsSpan()[..leftLength]);
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
    public class InlineObjectElement : InlineElement
    {
        public string Src { get; }
        public string Height { get; }
        public string? Command { get; }
        public InlineObjectElement(string src, string height, string? command)
        {
            Src = src;
            Height = height;
            Command = command;
        }

        private const string cmdFloatLeft = "left";
        private const string cmdBlock = "block";
        private const string cmdInline = "inline";
        public override string ToHtml()
        {
            string style1 = "float:right"; //默认行为（向后兼容）
            if (Command is not null) {
                var cmd = Command.ToLower();
                if (cmd == cmdFloatLeft)
                    style1 = "float:left";
                else if (cmd == cmdBlock)
                    style1 = "display:block;margin:auto";
                else if (cmd == cmdInline)
                    style1 = "display:inline;vertical-align:middle";
            }
            string style2 = $"height:{Height};";

            if (UrlUtil.IsImage(Src))
                return $"<img src=\"{Src}\" style=\"{style1};{style2}\"/>";
            else if (UrlUtil.IsAudio(Src))
                return $"<audio controls src=\"{Src}\" style=\"{style1};{style2}\"></audio>";
            else if (UrlUtil.IsVideo(Src))
                return $"<video controls src=\"{Src}\" style=\"{style1};{style2}\"></video>";
            return "";
        }
        public override void WriteHtml(StringBuilder sb)
        {
            sb.Append(ToHtml());
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
