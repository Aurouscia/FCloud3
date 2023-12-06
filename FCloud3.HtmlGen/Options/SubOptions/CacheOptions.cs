using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options.SubOptions
{
    public class CacheOptions
    {
        public bool UseCache { get; private set; }
        public int SlideExpirationMins { get; private set; }

        private readonly ParserBuilder _master;

        public CacheOptions(ParserBuilder master)
        {
            UseCache = true;
            SlideExpirationMins = 5;
            _master = master;
        }
        public ParserBuilder DisableCache()
        {
            UseCache = false;
            return _master;
        }
        public ParserBuilder SetSlideExpirationMins(int mins)
        {
            SlideExpirationMins = mins;
            return _master;
        }
    }
}
