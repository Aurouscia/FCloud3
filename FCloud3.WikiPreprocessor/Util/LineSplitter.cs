using Ganss.Xss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Util
{
    public class LineSplitter
    {
        private static Lazy<HtmlSanitizer> Sanitizer = new(() =>
        {
            var s = new HtmlSanitizer();
            s.AllowedAttributes.Add("class");
            s.AllowedTags.Add("style");
            return s;
        });
        public static List<LineAndHash> Split(string input, ILocatorHash? locatorHash)
        {
            var htmlRanges = HtmlArea.FindRanges(input);

            List<string> lines;
            StringBuilder sb = new();
            lines = new();
            int layer = 0;
            for(int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (htmlRanges.Any(x => i >= x.Start.Value && i <= x.End.Value))
                {
                    if(!Consts.htmlAvoidChars.Contains(c))
                        sb.Append(c);
                    continue;
                }
                if (c == Consts.tplt_L)
                    layer++;
                if (c == Consts.tplt_R)
                    layer--;
                if (c == Consts.lineSep && layer == 0)
                {
                    lines.Add(sb.ToString());
                    sb.Clear();
                }
                else
                    sb.Append(c);
            }
            lines.Add(sb.ToString());

            var hashedLines = lines.ConvertAll(x =>
            {
                x = x.Trim();
                string? hash = locatorHash?.Hash(x);
                return new LineAndHash(hash, Sanitizer.Value.Sanitize(x));
            });
            hashedLines.RemoveAll(line => string.IsNullOrWhiteSpace(line.Text));
            return hashedLines;
        }
    }
    public class LineAndHash
    {
        public string? RawLineHash { get; }
        public string Text { get; set; }
        public LineAndHash(string? rawLineHash, string content)
        {
            RawLineHash = rawLineHash;
            Text = content;
        }
    }
}
