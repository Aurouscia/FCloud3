using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCloud3.WikiPreprocessor.ConvertingProvider;
using FCloud3.WikiPreprocessor.ConvertingProvider.Models;

namespace FCloud3.WikiPreprocessor.Test.Support
{
    internal class ConvertingProviderBase : IScopedConvertingProvider
    {
        public virtual string? Implant(string implantSpan)
        {
            throw new NotImplementedException();
        }

        public virtual LinkItem? Link(string linkSpan)
        {
            throw new NotImplementedException();
        }

        public virtual string? Replace(string replaceTarget)
        {
            throw new NotImplementedException();
        }
    }
}
