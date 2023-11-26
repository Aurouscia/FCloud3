using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public static class Escape
    {
        public const char escapeChar = '\\';
        public const string removeEscapeCharOfPattern = @"\\(?!\\)";
        public static string HideEscapeMark(string input)
        {
            if (!input.Contains(escapeChar))
                return input;
            return Regex.Replace(input, removeEscapeCharOfPattern ,string.Empty);
        }
    }
}
