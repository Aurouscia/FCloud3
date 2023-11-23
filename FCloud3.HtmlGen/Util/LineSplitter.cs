using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public class LineSplitter
    {
        public static List<string> Split(string input,bool removeEmptyLine=true,bool trim=true)
        {
            int start = 0;
            int length = 0;
            List<string> lines = new();
            int layer = 0;
            foreach(var c in input)
            {
                if (c == Consts.tplt_L)
                    layer++;
                if (c == Consts.tplt_R)
                    layer--;
                if (c == '\n' && layer == 0)
                {
                    lines.Add(input.Substring(start, length));
                    start += length+1;
                    length = 0;
                }
                else
                    length++;
            }
            lines.Add(input.Substring(start));

            if(trim)
                lines = lines.ConvertAll(x => x.Trim());
            if(removeEmptyLine)
                lines.RemoveAll(string.IsNullOrWhiteSpace);
            return lines;
        }
    }
}
