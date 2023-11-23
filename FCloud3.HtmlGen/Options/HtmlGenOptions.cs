using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class HtmlGenOptions
    {
        public List<HtmlTemplate> Templates { get; set; }
        public List<HtmlInlineRule> InlineRules { get; set; }
        public List<HtmlTypedBlockRule> TypedBlockRules { get; set; }

        public HtmlGenOptions()
        {
            Templates = new();
            InlineRules = new();
            TypedBlockRules = new();

            InlineRules.Sort((x, y) => y.MarkLeft.Length - x.MarkRight.Length);
        }
    }
}
