using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Rules
{
    public interface IRule
    {
        public string GetStyles();
        public string GetPreScripts();
        public string GetPostScripts();
        public bool IsSingleUse { get; }
        public string UniqueName { get; }
    }

    public static class RuleListExtensions
    {
        public static void WriteStyles(this List<IRule> list, StreamWriter sw)
        {
            list.ForEach(x => sw.Write(x.GetStyles()));
        }
        public static void WritePreScripts(this List<IRule> list, StreamWriter sw)
        {
            list.ForEach(x => sw.Write(x.GetPreScripts()));
        }
        public static void WritePostScripts(this List<IRule> list, StreamWriter sw)
        {
            list.ForEach(x => sw.Write(x.GetPostScripts()));
        }
    }
}
