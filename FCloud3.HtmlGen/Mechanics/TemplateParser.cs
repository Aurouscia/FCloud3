using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using System.Text.RegularExpressions;

namespace FCloud3.HtmlGen.Mechanics
{
    public class TemplateParser
    {
        private readonly HtmlGenOptions _options;
        private readonly BlockParser _blockParser;

        public TemplateParser(HtmlGenOptions options)
        {
            _options = options;
            _blockParser = new(options);
        }

        private const string templateCallPattern = @"\{\{.*?\}(.|\n)*?\}";

        public ElementCollection RunMacro(string input)
        {
            var matches = Regex.Matches(input,templateCallPattern,RegexOptions.Compiled);

            var res = new ElementCollection();

            int pointer = 0;
            foreach(Match match in matches.AsEnumerable())
            {
                int index = match.Index;
                string value = match.Value;
                if (value.Contains('\n'))
                {
                    string beforeMatch = input.Substring(pointer, index - pointer);
                    if (beforeMatch.Length > 0)
                    {
                        ElementCollection beforeMatchElements = _blockParser.Run(beforeMatch);
                        res.AddRange(beforeMatchElements);
                    }
                    res.Add(ParseSingle(value));
                    pointer = index + value.Length;
                }
            }
            string last = input.Substring(pointer);
            if(last.Length > 0)
            {
                ElementCollection lastElements = _blockParser.Run(last);
                res.AddRange(lastElements);
            }
            return res;
        }

        private static readonly string[] valuesSep = new string[] { "&&" , "\n"};
        private static readonly string keyValueSep = "::";
        public Element ParseSingle(string templateCallSource)
        {
            int firstAntiBrace = templateCallSource.IndexOf('}');
            string templateName = templateCallSource.Substring(2, firstAntiBrace-2).Trim();
            var template = _options.Templates.Find(x => x.Name == templateName);
            if (template is null)
                return new ErrorElement($"找不到指定模板({templateName})");

            string valueStr = templateCallSource.Substring(firstAntiBrace + 1, templateCallSource.Length - 2).Trim();
            List<string> values = valueStr.Split(valuesSep,StringSplitOptions.TrimEntries).ToList();
            Dictionary<string, string> valuesDic = new();

            if(values.Count == 1 && !values[0].Contains(keyValueSep))
            {
                string key = string.Empty;
                string value = values[0].Trim();
                valuesDic.Add(key,value);
            }
            else
            {
                foreach (var str in values)
                {
                    string[] parts = str.Split(keyValueSep);
                    if (parts.Length != 2)
                        break;
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    valuesDic.Add(key, value);
                }
            }
            return new TemplateElement(template, valuesDic);
        }
    }
}
