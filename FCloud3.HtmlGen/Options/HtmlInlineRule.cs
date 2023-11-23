using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class HtmlInlineRule
    {
        public string Name { get; set; }
        public string MarkLeft { get; set; }
        public string MarkRight { get; set; }
        public string PutLeft { get; set; }
        public string PutRight { get; set; }
        public bool Greedy { get; set; }
    }
}
