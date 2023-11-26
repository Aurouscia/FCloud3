using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class AutoReplaceOptions
    {
        public List<string> Detects { get; }
        public Func<string, string> Replace { get; }
        public AutoReplaceOptions()
        {
            Detects = new();
            Replace = x => x;
        }
        public AutoReplaceOptions(List<string> targets, Func<string, string> replace)
        {
            Detects = targets;
            Replace = replace;
        }
    }
}
