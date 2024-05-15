using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Rules
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
            => list.ForEach(x => sw.Write(x.GetStyles()));
        public static void WritePreScripts(this List<IRule> list, StreamWriter sw) 
            => list.ForEach(x => sw.Write(x.GetPreScripts()));
        public static void WritePostScripts(this List<IRule> list, StreamWriter sw) 
            => list.ForEach(x => sw.Write(x.GetPostScripts()));
        public static void WriteStyles(this List<IRule> list, StringBuilder sb)
            => list.ForEach(x => sb.Append(x.GetStyles()));
        public static void WritePreScripts(this List<IRule> list, StringBuilder sb)
            => list.ForEach(x => sb.Append(x.GetPreScripts()));
        public static void WritePostScripts(this List<IRule> list, StringBuilder sb)
            => list.ForEach(x => sb.Append(x.GetPostScripts()));
        public static void FilterAllWithCommons(this List<IRule> list)
        {
            list.RemoveAll(x =>
                string.IsNullOrWhiteSpace(x.GetStyles()) &&
                string.IsNullOrWhiteSpace(x.GetPreScripts()) &&
                string.IsNullOrWhiteSpace(x.GetPostScripts())
            );
        }
    }
}
