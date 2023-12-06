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
    public class Parser
    {
        private readonly ParserContext _ctx;
        private readonly BlockParser _blockParser;
        public const int maxInputLength = 10000;
        public const string maxInputLengthViolatedHint = "单段字数过多，请分段";
        public Parser(ParserOptions options)
        {
            _ctx = new(options);
            _blockParser = new(_ctx);
        }
        public string RunToPlain(string? input,bool putCommon = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            if (input.Length > maxInputLength)
                return maxInputLengthViolatedHint;
            IHtmlable result = _blockParser.Run(input);
            if (!putCommon)
                return result.ToHtml();
            else
                return $"{Styles()}{PreScripts()}{result.ToHtml()}{PostScripts()}";
        }
        public ParserResult RunToStructured(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new();
            if (input.Length > maxInputLength)
                return new(maxInputLengthViolatedHint);
            IHtmlable htmlable = _blockParser.Run(input);
            string resultStr = htmlable.ToHtml();
            ScriptExtract.Run(resultStr, out string content, out string extractedScripts);
            string preScripts = PreScripts(false);
            string postScripts = $"{extractedScripts}\n{PostScripts(false)}";
            string styles = Styles(false);
            ParserResult result = new(content, preScripts, postScripts, styles);
            return result;
        }
        public string RunMutiple(List<string?> inputs)
        {
            List<string> res = inputs.ConvertAll(x=>RunToPlain(x,false));
            throw new NotImplementedException();
            //return $"{Styles()}{PreScripts()}{string.Concat(res)}{PostScripts()}";
        }
        private string Styles(bool withLabel=true)
        {
            var allStyles = _ctx.GetUsedRules().Select(x => x.GetStyles()).ToList();
            if (allStyles.Count == 0 || allStyles.All(s => s == ""))
                return "";
            allStyles = allStyles.Distinct().ToList();
            string res = string.Concat(allStyles);
            if(withLabel)
                return HtmlLabel.Style(res);
            return res;
        }
        private string PreScripts(bool withLabel= true)
        { 
            var allPre = _ctx.GetUsedRules().Select(x=>x.GetPreScripts()).ToList();
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
            var allPost = _ctx.GetUsedRules().Select(x => x.GetPostScripts()).ToList();
            if (allPost.Count == 0 || allPost.All(x => x == ""))
                return "";
            allPost = allPost.Distinct().ToList();
            string res = string.Join('\n',allPost);
            if(withLabel)
                return HtmlLabel.Script(res);
            return res;
        }
    }

    public class ParserResult
    {
        public string Content { get; }
        public string PreScript { get; }
        public string PostScript { get; }
        public string Style { get; }
        public ParserResult(string content="", string preScript="", string postScript="",string style="")
        {
            Content = content;
            PreScript = preScript;
            PostScript = postScript;
            Style = style;
        }
    }
}
