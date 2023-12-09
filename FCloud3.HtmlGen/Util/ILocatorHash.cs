using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public interface ILocatorHash
    {
        public string? Hash(string? input);
    }
}
