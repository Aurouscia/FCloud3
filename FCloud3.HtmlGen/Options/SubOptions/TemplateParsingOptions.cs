using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class TemplateParsingOptions : IHtmlGenOptions
    {
        public List<HtmlTemplate> Templates { get; }
        public TemplateParsingOptions(List<HtmlTemplate>? templates = null)
        {
            Templates = templates ?? new();
        }

        public void OverrideWith(IHtmlGenOptions another)
        {
            if(another is TemplateParsingOptions tpo)
            {
                Templates.RemoveAll(x=>tpo.Templates.Contains(x));
                Templates.AddRange(tpo.Templates);  
            }
        }
    }
}
