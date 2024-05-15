using FCloud3.WikiPreprocessor.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class TemplateParsingOptions
    {
        public List<Template> Templates { get; }
        private readonly ParserBuilder _master;
        public TemplateParsingOptions(ParserBuilder master, List<Template>? templates = null)
        {
            Templates = templates ?? new();
            _master = master;
        }

        public ParserBuilder AddTemplates(List<Template> templates)
        {
            Templates.RemoveAll(x=>templates.Contains(x));
            Templates.AddRange(templates);  
            return _master;
        }
        public ParserBuilder ClearTemplates()
        {
            Templates.Clear();
            return _master;
        }
    }
}
