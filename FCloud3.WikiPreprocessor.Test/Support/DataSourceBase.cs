using FCloud3.WikiPreprocessor.DataSource;
using FCloud3.WikiPreprocessor.DataSource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Test.Support
{
    internal class DataSourceBase : IScopedDataSource
    {
        public virtual string? Implant(string implantSpan)
        {
            throw new NotImplementedException();
        }

        public virtual LinkItem? Link(string linkSpan)
        {
            throw new NotImplementedException();
        }
    }
}
