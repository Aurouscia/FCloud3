using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen
{
    public interface IHtmlable
    {
        public string ToHtml();
        public void WriteHtml(StringBuilder sb);
        public List<IRule>? ContainRules();
        public List<IHtmlable>? ContainFootNotes();
    }
}
