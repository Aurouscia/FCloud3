using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class AutoReplaceOptions
    {
        public HashSet<ReplaceTarget> Detects { get; private set; }
        private readonly ParserBuilder _master;
        public AutoReplaceOptions(ParserBuilder master)
        {
            Detects = [];
            _master = master;
        }
        public ParserBuilder AddReplacingTargets(IEnumerable<string> targets, bool isSingle)
        {
            foreach(var t in targets)
                Detects.Add(new ReplaceTarget(t, isSingle));
            return _master;
        }
    }
    
    public class ReplaceTarget(string text, bool isSingleUse)
    {
        public string Text { get; } = text;
        public bool IsSingleUse { get; } = isSingleUse;
        public override int GetHashCode() => Text.GetHashCode();
        public override bool Equals(object? obj)
        {
            if (obj is ReplaceTarget t)
                return t.Text == this.Text;
            return false;
        }
    }
}
