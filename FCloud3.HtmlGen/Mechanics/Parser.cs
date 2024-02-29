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
            string resStr = result.ToHtml();
            if (_ctx.Options.Debug)
            {
                resStr = _ctx.DebugInfo() + resStr;
            }
            if (!putCommon)
                return resStr;
            else
                return $"{Styles()}{PreScripts()}{resStr}{PostScripts()}";
        }
        public ParserResult RunToStructured(string? input)
        {
            _ctx.Reset();
            if (string.IsNullOrWhiteSpace(input))
                return new();
            if (input.Length > maxInputLength)
                return new(input);
            IHtmlable htmlable = _blockParser.Run(input);
            string content = htmlable.ToHtml();
            string preScripts = PreScripts(false);
            string postScripts = PostScripts(false);
            string styles = Styles(false);
            string footNotes = FootNotes();
            if (_ctx.Options.Debug)
            {
                content = _ctx.DebugInfo() + content;
            }
            ParserResult result = new(content, preScripts, postScripts, styles, footNotes);
            return result;
        }
        public IHtmlable RunToRaw(string? input)
        {
            _ctx.Reset();
            if(input is null)
                return new EmptyElement();
            if (input.Length > maxInputLength)
                return new TextElement(input);
            IHtmlable htmlable = _blockParser.Run(input);
            return htmlable;
        }
        public string RunMutiple(List<string?> inputs)
        {
            List<string> res = inputs.ConvertAll(x=>RunToPlain(x,false));
            throw new NotImplementedException();
            //return $"{Styles()}{PreScripts()}{string.Concat(res)}{PostScripts()}";
        }
        private string Styles(bool withLabel=true)
        {
            var allStyles = _ctx.RuleUsage.GetUsedRules().Select(x => x.GetStyles()).ToList();
            if (allStyles.Count == 0 || allStyles.All(s => s == ""))
                return "";
            allStyles = allStyles.Distinct().ToList();
            allStyles.Sort();
            string res = string.Concat(allStyles);
            if(withLabel)
                return HtmlLabel.Style(res);
            return res;
        }
        private string PreScripts(bool withLabel= true)
        { 
            var allPre = _ctx.RuleUsage.GetUsedRules().Select(x=>x.GetPreScripts()).ToList();
            if (allPre.Count == 0 || allPre.All(x=>x==""))
                return "";
            allPre = allPre.Where(x=>x!="").Distinct().ToList();
            string res = string.Join('\n',allPre);
            if(withLabel)
                return HtmlLabel.Script(res);
            return res;
        }
        private string PostScripts(bool withLabel= true)
        {
            var allPost = _ctx.RuleUsage.GetUsedRules().Select(x => x.GetPostScripts()).ToList();
            if (allPost.Count == 0 || allPost.All(x => x == ""))
                return "";
            allPost = allPost.Distinct().ToList();
            string res = string.Join('\n',allPost);
            if(withLabel)
                return HtmlLabel.Script(res);
            return res;
        }
        private string FootNotes()
        {
            var allFootNotes = _ctx.FootNote.FootNoteBodys;
            StringBuilder sb = new("<div class=\"refbodies\">");
            allFootNotes.ForEach(x =>
            {
                sb.Append(x.ToHtml());
            });
            sb.Append("</div>");
            return sb.ToString();
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
}
