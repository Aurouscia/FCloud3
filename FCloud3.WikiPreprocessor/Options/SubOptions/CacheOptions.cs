using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class CacheOptions
    {
        public bool UseCache { get; private set; }

        private readonly ParserBuilder _master;

        public CacheOptions(ParserBuilder master)
        {
            UseCache = false;
            _master = master;
        }
        public ParserBuilder EnableCache()
        {
            UseCache = true;
            return _master;
        }
    }
}
