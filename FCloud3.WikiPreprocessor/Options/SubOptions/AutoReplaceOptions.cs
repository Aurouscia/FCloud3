using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class AutoReplaceOptions
    {
        public List<ReplaceTarget> Detects { get; private set; }
        public Func<string, string> Replace { get; private set; }
        private readonly ParserBuilder _master;
        public AutoReplaceOptions(ParserBuilder master)
        {
            Detects = new();
            Replace = x => x;
            _master = master;
        }
        public ParserBuilder AddReplacing(List<string> targets, Func<string, string> replace, bool isSingle = true)
        {
            Detects.RemoveAll(x=> targets.Contains(x.Text));
            targets.ForEach(x =>
            {
                Detects.Add(new ReplaceTarget(x, isSingle));
            });
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
    
    public readonly struct ReplaceTarget
    {
        public string Text { get; }
        public bool IsSingleUse { get; }
        public ReplaceTarget(string text, bool isSingleUse) { Text = text; IsSingleUse = isSingleUse; }
    }
}
