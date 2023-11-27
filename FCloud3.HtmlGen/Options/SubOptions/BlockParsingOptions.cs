using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class BlockParsingOptions : IHtmlGenOptions
    {
        public List<IHtmlBlockRule> BlockRules { get; private set; }
        public int TitleLevelOffset { get; private set; }
        public BlockParsingOptions(List<IHtmlBlockRule>? blockRules = null, int titleLevelOffset = 1)
        {
            BlockRules = blockRules ?? new();
            TitleLevelOffset = titleLevelOffset;
        }

        public void OverrideWith(IHtmlGenOptions another)
        {
            if(another is BlockParsingOptions bpo)
            {
                BlockRules.RemoveAll(bpo.BlockRules.Contains);
                BlockRules.AddRange(bpo.BlockRules);
                TitleLevelOffset = bpo.TitleLevelOffset;
            }
        }
    }
}
