using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class ImplantsHandleOptions : IHtmlGenOptions
    {
        public Func<string, string?> HandleImplant { get; private set; }
        public ImplantsHandleOptions()
        {
            HandleImplant = x => x;
        }
        public ImplantsHandleOptions(Func<string,string?> handler)
        {
            HandleImplant = handler;
        }

        public void OverrideWith(IHtmlGenOptions another)
        {
            if(another is ImplantsHandleOptions iho)
            {
                var original = HandleImplant;
                HandleImplant = (x) =>
                {
                    string? newAnswer = iho.HandleImplant(x);
                    if (newAnswer is null)
                        return original(x);
                    return newAnswer;
                };
            }
        }
    }
}
