using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Diff.StringDiff
{
    public class StringDiff(int index, string oriContent, string newContent)
    {
        public int Index { get; set; } = index;
        public string Ori { get; set; } = oriContent;
        public string New { get; set; } = newContent;

        public string Up(string oriStr, int offset = 0)
        {
            int replacedLength = Ori.Length;
            string resLeft = oriStr.Substring(0, Index + offset);
            string resRight = oriStr.Substring(Index + offset + replacedLength);
            return resLeft + New + resRight;
        }
        public string Down(string newStr, int offset = 0)
        {
            int replacedLength = New.Length;
            string resLeft = newStr.Substring(0, Index);
            string resRight = newStr.Substring(Index + replacedLength);
            return resLeft + Ori + resRight;
        }
    }
}
