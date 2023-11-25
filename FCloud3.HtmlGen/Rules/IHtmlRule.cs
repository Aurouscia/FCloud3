using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Rules
{
    public interface IHtmlRule
    {
        public string GetPreCommons();
        public string GetPostCommons();
    }
}
