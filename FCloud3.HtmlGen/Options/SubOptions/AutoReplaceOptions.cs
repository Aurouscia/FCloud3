using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class AutoReplaceOptions
    {
        public List<string> Detects { get; private set; }
        public Func<string, string> Replace { get; private set; }
        private readonly ParserBuilder _master;
        public AutoReplaceOptions(ParserBuilder master)
        {
            Detects = new();
            Replace = x => x;
            _master = master;
        }

        public ParserBuilder AddReplacing(List<string> targets, Func<string, string> replace)
        {
            Detects.RemoveAll(targets.Contains);
            Detects.AddRange(targets);
            var original = Replace;
            Replace = (d) =>
            {
                string newAnswer = replace(d);
                if (newAnswer == d)
                    return original(d);
                return newAnswer;
            };
            return _master;
        }
    }
}
