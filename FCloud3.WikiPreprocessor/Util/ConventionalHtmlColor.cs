using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetColorParser;

namespace FCloud3.WikiPreprocessor.Util
{
    public static class ConventionalHtmlColor
    {
        public static bool TryFormalize(string s, IColorParser colorParser, out string res)
        {
            Color c;
            s = s.ToLower();
            var success = false;
            if (!colorParser.TryParseColor(s, out c))
            {
                if (s.Contains(','))
                {
                    s = $"rgb({s})";
                    success = colorParser.TryParseColor(s, out c);
                }
            }
            else
                success = true;
            if (success)
            {
                res = $"rgb({c.R},{c.G},{c.B})";
                return true;
            }
            else
            {
                res = "";
                return false;
            }
        }
    }
}
