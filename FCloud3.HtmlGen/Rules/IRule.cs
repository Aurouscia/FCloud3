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
    }
}
