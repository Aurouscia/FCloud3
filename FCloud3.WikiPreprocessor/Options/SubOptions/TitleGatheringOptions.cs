using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class TitleGatheringOptions
    {
        public bool Enabled { get; private set; }

        private readonly ParserBuilder _master;
        public TitleGatheringOptions(ParserBuilder master)
        {
            _master = master;
            Enabled = false;
        }
        public ParserBuilder Enable()
        {
            Enabled = true;
            return _master;
        }
    }
}
