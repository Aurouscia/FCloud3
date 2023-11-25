using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
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
        public string Run(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            ElementCollection result = _blockParser.Run(input);
            return result.ToHtml();
        }
    }
}
