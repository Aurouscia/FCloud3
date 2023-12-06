using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public static partial class Escape
    {
        public const char escapeChar = '\\';
        public static string HideEscapeMark(string input)
        {
            if (!input.Contains(escapeChar))
                return input;
            return RemoveEscapeChar().Replace(input, string.Empty);
        }

        [GeneratedRegex("\\\\(?!\\\\)")]
        private static partial Regex RemoveEscapeChar();
    }
}
