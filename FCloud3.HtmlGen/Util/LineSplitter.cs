using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public class LineSplitter
    {
        public static List<LineAndHash> Split(string input, ILocatorHash? locatorHash,bool removeEmptyLine=true,bool trim=true)
        {
            List<string> lines;
            if (!input.Contains(Consts.tplt_L) || !input.Contains(Consts.tplt_R))
            {
                lines = input.Split(Consts.lineSep).ToList();
            }
            else {
                int start = 0;
                int length = 0;
                lines = new();
                int layer = 0;
                foreach (var c in input)
                {
                    if (c == Consts.tplt_L)
                        layer++;
                    if (c == Consts.tplt_R)
                        layer--;
                    if (c == Consts.lineSep && layer == 0)
                    {
                        lines.Add(input.Substring(start, length));
                        start += length + 1;
                        length = 0;
                    }
                    else
                        length++;
                }
                lines.Add(input.Substring(start));
            }

            var hashedLines = lines.ConvertAll(x =>
            {
                if (trim)
                    x = x.Trim();
                string? hash = locatorHash?.Hash(x);
                return new LineAndHash(hash, x);
            });
            if(removeEmptyLine)
                hashedLines.RemoveAll(line=>string.IsNullOrWhiteSpace(line.Text));
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
