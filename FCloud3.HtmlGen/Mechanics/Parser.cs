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
        private readonly HtmlGenOptions _options;
        private readonly BlockParser _blockParser;
        public Parser(HtmlGenOptions options)
        {
            _options = options;
            _blockParser = new(options);
        }
        public string Run(string? input,bool putCommon = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            ElementCollection result = _blockParser.Run(input);
            if (!putCommon)
                return result.ToHtml();
            else
                return $"{Styles()}{PreScripts()}{result.ToHtml()}{PostScripts()}";
        }
        public string RunMutiple(List<string?> inputs)
        {
            List<string> res = inputs.ConvertAll(x=>Run(x,false));
            return $"{Styles()}{PreScripts()}{string.Concat(res)}{PostScripts()}";
        }
        public string Styles()
        {
            var allStyles = _options.UsedRulesLog.Select(x => x.GetStyles()).ToList();
            if (allStyles.Count == 0 || allStyles.All(x=>x==""))
                return "";
            allStyles = allStyles.Distinct().ToList();
            return HtmlLabel.Style(string.Concat(allStyles));
        }
        public string PreScripts()
        { 
            var allPre = _options.UsedRulesLog.Select(x=>x.GetPreScripts()).ToList();
            if (allPre.Count == 0 || allPre.All(x=>x==""))
                return "";
            allPre = allPre.Distinct().ToList();
            return HtmlLabel.Script(string.Concat(allPre));
        }
        public string PostScripts()
        {
            var allPost = _options.UsedRulesLog.Select(x => x.GetPostScripts()).ToList();
            if (allPost.Count == 0 || allPost.All(x => x == ""))
                return "";
            allPost = allPost.Distinct().ToList();
            return HtmlLabel.Script(string.Concat(allPost));
        }
    }
}
