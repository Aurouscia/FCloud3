using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Mechanics
{
    public class InlineParser
    {
        private readonly HtmlGenOptions _options;
        public InlineParser(HtmlGenOptions options) 
        {
            _options = options;
        }

        public ElementCollection RunForLine(string input)
        {
            ElementCollection lineContent = new(new TextElement(input));
            LineElement line = new(lineContent);
            ElementCollection res = new(line);

            return res;
        }
    }
}
