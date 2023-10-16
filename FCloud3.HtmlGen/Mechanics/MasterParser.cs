using FCloud3.HtmlGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Mechanics
{
    public class MasterParser
    {
        private readonly HtmlGenOptions _options;
        private readonly TemplateParser _templateParser;

        public MasterParser(HtmlGenOptions options)
        {
            _options = options;
            _templateParser = new TemplateParser(options);
        }
        public string Run(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            return _templateParser.RunMacro(input).ToHtml();
        }
    }
}
