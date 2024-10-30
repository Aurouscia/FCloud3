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
        private readonly ParserBuilder _master;
        public AutoReplaceOptions(ParserBuilder master)
        {
            Detects = new();
            _master = master;
        }
        public ParserBuilder AddReplacingTargets(List<string> targets, bool isSingle)
        {
            Detects.RemoveAll(x=> targets.Contains(x.Text));
            targets.ForEach(x =>
            {
                Detects.Add(new ReplaceTarget(x, isSingle));
            });
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
