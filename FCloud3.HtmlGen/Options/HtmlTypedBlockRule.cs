using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class HtmlTypedBlockRule
    {
        public string Mark { get; set; }
        public string PutLeft { get; set; }
        public string PutRight { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is HtmlTypedBlockRule rule)
                return rule.Mark == this.Mark;
            return false;
        }

        public override int GetHashCode()
        {
            return Mark.GetHashCode();
        }
    }
}
