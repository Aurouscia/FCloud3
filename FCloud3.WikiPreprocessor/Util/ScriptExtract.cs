using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Util
{
    public static partial class ScriptExtract
    {
        public static void Run(string input,out string pureContent,out string scripts)
        {
            var matches = ScriptRegex().Matches(input);
            var matchValues = string.Join('\n', matches.AsEnumerable().Select(x => x.Value));
            scripts = string.Join("\n", matchValues);
            pureContent = ScriptRegexInludingLabel().Replace(input, "");
        }

        [GeneratedRegex("(?<=<script[^</>]{0,19}>)(.|\n)*?(?=</script>)", RegexOptions.IgnoreCase, 200)]
        private static partial Regex ScriptRegex();

        [GeneratedRegex("<script[^</>]{0,19}>(.|\n)*?</script>")]
        private static partial Regex ScriptRegexInludingLabel();
    }
}
