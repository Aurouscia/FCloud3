using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class AutoReplaceOptions : IHtmlGenOptions
    {
        public List<string> Detects { get; private set; }
        public Func<string, string> Replace { get; private set; }
        public AutoReplaceOptions()
        {
            Detects = new();
            Replace = x => x;
        }
        public AutoReplaceOptions(List<string> targets, Func<string, string> replace)
        {
            Detects = targets;
            Replace = replace;
        }

        public void OverrideWith(IHtmlGenOptions another)
        {
            if (another is AutoReplaceOptions aro) 
            {
                Detects.RemoveAll(aro.Detects.Contains);
                Detects.AddRange(aro.Detects);
                var original = Replace;
                Replace = (d) =>
                {
                    string newAnswer = aro.Replace(d);
                    if (newAnswer == d)
                        return original(d);
                    return newAnswer;
                };
            }
        }
    }
}
