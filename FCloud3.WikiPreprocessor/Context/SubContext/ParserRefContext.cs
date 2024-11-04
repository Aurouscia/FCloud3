using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    public class ParserRefContext
    {
        private HashSet<string> Refs { get; set; } = [];
        public void Clear() => Refs.Clear();
        public void ReportRef(string? str)
        {
            str = str?.Trim();
            if(!string.IsNullOrWhiteSpace(str) && str.Length <= 32)
                Refs.Add(str);
        }
        public IEnumerable<string> GetRefs() => Refs; 
    }
}
