using FCloud3.WikiPreprocessor.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class BlockParsingOptions
    {
        public List<IBlockRule> BlockRules { get; private set; }
        public int TitleLevelOffset { get; private set; }
        private readonly ParserBuilder _master;
        public BlockParsingOptions(ParserBuilder master)
        {
            _master = master;
            BlockRules = new();
        }

        public ParserBuilder AddMoreRules(List<IBlockRule> blockRules)
        {
            BlockRules.RemoveAll(blockRules.Contains);
            BlockRules.AddRange(blockRules);
            return _master;
        }
        public ParserBuilder AddMoreRule(IBlockRule rule)
        {
            BlockRules.Remove(rule);
            BlockRules.Add(rule);
            return _master;
        }

        public ParserBuilder SetTitleLevelOffset(int titleLevelOffset)
        {
            if(titleLevelOffset<0 || titleLevelOffset>4)
                throw new ArgumentOutOfRangeException(nameof(titleLevelOffset));
            TitleLevelOffset = titleLevelOffset;
            return _master;
        }
    }
}
