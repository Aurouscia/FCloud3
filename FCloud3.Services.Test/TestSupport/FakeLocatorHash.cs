using FCloud3.WikiPreprocessor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Test.TestSupport
{
    internal class FakeLocatorHash : ILocatorHash
    {
        public string? Hash(string? input)
        {
            return input?.GetHashCode().ToString();
        }
    }
}
