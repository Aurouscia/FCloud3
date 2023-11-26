using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class ImplantsHandleOptions
    {
        public Func<string, string?> HandleImplant { get; }
        public ImplantsHandleOptions()
        {
            HandleImplant = x => x;
        }
        public ImplantsHandleOptions(Func<string,string?> handler)
        {
            HandleImplant = handler;
        }
    }
}
