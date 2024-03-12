using FCloud3.HtmlGen.Context;
using FCloud3.HtmlGen.Context.SubContext;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Mechanics
{
    public class Parser:IDisposable
    {
        private readonly ParserContext _ctx;
        private readonly BlockParser _blockParser;
        public const int maxInputLength = 5000;
        public ParserContext Context { get { return _ctx; } }
        public Parser(ParserOptions options)
        {
            _ctx = new(options);
            _blockParser = new(_ctx);
        }
        public Parser(ParserContext ctx)
        {
            _ctx = ctx;
            _blockParser = new(_ctx);
        }
        public string RunToPlain(string? input,bool putCommon = false)
        {
            _ctx.Reset();
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            if (input.Length > maxInputLength)
                return input;
            IHtmlable result = _blockParser.Run(input);
            StringBuilder resSb = new();
            if (_ctx.Options.Debug)
                    resSb.AppendLine(_ctx.DebugInfo());
            if (!putCommon)
            {
                result.WriteHtml(resSb);
                return resSb.ToString();
            }
            else
            {
                Styles(resSb);
                PreScripts(resSb);
                result.WriteHtml(resSb);
                PostScripts(resSb);
                FootNotes(resSb);
            }
            return resSb.ToString();
        }
        public ParserResult RunToParserResult(string? input)
        {
            _ctx.Reset();
            if (string.IsNullOrWhiteSpace(input))
                return new();
            if (input.Length > maxInputLength)
                return new(input);
            IHtmlable htmlable = _blockParser.Run(input);
            StringBuilder resSb = new();
            if (_ctx.Options.Debug)
                resSb.AppendLine(_ctx.DebugInfo());
            htmlable.WriteHtml(resSb);
            string content = resSb.ToString();resSb.Clear();
            PreScripts(resSb, false);
            string preScripts = resSb.ToString();resSb.Clear();
            PostScripts(resSb, false);
            string postScripts = resSb.ToString();resSb.Clear();
            Styles(resSb, false);
            string styles = resSb.ToString();resSb.Clear();
            FootNotes(resSb);
            string footNotes = resSb.ToString();

            ParserResult result = new(content, preScripts, postScripts, styles, footNotes);
            return result;
        }
        public ParserResultRaw RunToParserResultRaw(string? input, bool enforceBlock = true)
        {
            _ctx.Reset();
            if (string.IsNullOrWhiteSpace(input))
                return new();
            if (input.Length > maxInputLength)
                return new(input);
            IHtmlable htmlable = _blockParser.Run(input, enforceBlock);
            StringBuilder resSb = new();
            if (_ctx.Options.Debug)
                resSb.AppendLine(_ctx.DebugInfo());
            htmlable.WriteHtml(resSb);

            ParserResultRaw result = new(
                content: resSb.ToString(),
                usedRules: _ctx.RuleUsage.GetUsedRules(),
                footNotes: _ctx.FootNote.AllToString(),
                titles: htmlable.ContainTitleNodes());
            return result;
        }
        public IHtmlable RunToObject(string? input)
        {
            _ctx.Reset();
            if(input is null)
                return new EmptyElement();
            if (input.Length > maxInputLength)
                return new TextElement(input);
            IHtmlable htmlable = _blockParser.Run(input);
            return htmlable;
        }

        public void WrapSection(
            string? title, List<ParserTitleTreeNode> subTitles, out ParserTitleTreeNode processed, out int titleId)
        {
            titleId = Context.TitleGathering.GenerateTitleId();
            processed = new(0, title ?? "??", titleId);
            processed.Subs = subTitles;
        }

        private void Styles(StringBuilder sb, bool withLabel=true)
        {
            var allStyles = _ctx.RuleUsage.GetUsedRules().Select(x => x.GetStyles()).ToList();
            if (allStyles.Count == 0 || allStyles.All(s => s == ""))
                return;
            allStyles = allStyles.Distinct().ToList();
            if (withLabel)
                sb.Append("<style>");
            allStyles.Sort();
            allStyles.ForEach(s =>
            {
                sb.Append(s);
            });
            if (withLabel)
                sb.Append("</style>");
        }
        private void PreScripts(StringBuilder sb, bool withLabel= true)
        { 
            var allPre = _ctx.RuleUsage.GetUsedRules().Select(x=>x.GetPreScripts()).ToList();
            if (allPre.Count == 0 || allPre.All(x=>x==""))
                return;
            allPre = allPre.Where(x=>x!="").Distinct().ToList();
            if (withLabel)
                sb.Append("<script>");
            allPre.ForEach(s =>
            {
                sb.Append(s);
                sb.Append('\n');
            });
            if (withLabel)
                sb.Append("</script>");
        }
        private void PostScripts(StringBuilder sb, bool withLabel= true)
        {
            var allPost = _ctx.RuleUsage.GetUsedRules().Select(x => x.GetPostScripts()).ToList();
            if (allPost.Count == 0 || allPost.All(x => x == ""))
                return;
            allPost = allPost.Distinct().ToList();
            if (withLabel)
                sb.Append("<script>");
            allPost.ForEach(s =>
            {
                sb.Append(s);
                sb.Append('\n');
            });
            if (withLabel)
                sb.Append("</script>");
        }
        private void FootNotes(StringBuilder sb)
        {
            var allFootNotes = _ctx.FootNote.FootNoteBodys;
            if (allFootNotes.Count == 0) return;
            sb.Append("<div class=\"refbodies\">");
            allFootNotes.ForEach(x =>
            {
                x.WriteHtml(sb);
            });
            sb.Append("</div>");
        }
        ~Parser()
        {
            Dispose();
        }
        public void Dispose()
        {
            _ctx.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class ParserResult
    {
        public string Content { get; }
        public string PreScript { get; }
        public string PostScript { get; }
        public string Style { get; }
        public string FootNotes { get; }
        public ParserResult(string content="", string preScript="", string postScript="",string style="",string footNotes="")
        {
            Content = content;
            PreScript = preScript;
            PostScript = postScript;
            Style = style;
            FootNotes = footNotes;
        }
    }

    public class ParserResultRaw
    {
        public string Content { get; }
        public List<IRule> UsedRules { get; private set; }
        public List<string> FootNotes { get; private set; }
        public List<ParserTitleTreeNode> Titles { get; private set; }
        public ParserResultRaw(string content="", List<IRule>? usedRules=null, List<string>? footNotes=null, List<ParserTitleTreeNode>? titles=null)
        {
            Content = content;
            UsedRules = usedRules ?? new();
            FootNotes = footNotes ?? new();
            Titles = titles ?? new();
        }
        public List<IRule> UsedRulesWithCommons()
        {
            return UsedRules.Where(x =>
            {
                if (!string.IsNullOrEmpty(x.GetStyles()))
                    return true;
                if (!string.IsNullOrEmpty(x.GetPostScripts()))
                    return true;
                if (!string.IsNullOrEmpty(x.GetPreScripts()))
                    return true;
                return false;
            }).ToList();
        }
        public List<string> UsedRuleWithCommonsNames()
        {
            return UsedRulesWithCommons().ConvertAll(x => x.UniqueName);
        }
        public void MergeIn(ParserResultRaw another)
        {
            UsedRules = UsedRules.Union(another.UsedRules).ToList();
            FootNotes = FootNotes.Union(another.FootNotes).ToList();
        }
    }
}
