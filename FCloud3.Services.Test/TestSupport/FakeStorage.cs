using FCloud3.Services.Files.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Test.TestSupport
{
    public class FakeStorage : IStorage
    {
        public StorageProvideType ProvideType => throw new NotImplementedException();

        public bool Delete(string pathName, out string? errmsg)
        {
            throw new NotImplementedException();
        }

        public string FullUrl(string pathName)
        {
            throw new NotImplementedException();
        }

        public Stream? Read(string pathName)
        {
            throw new NotImplementedException();
        }

        public bool Save(Stream s, string pathName, out string? errmsg)
        {
            throw new NotImplementedException();
        }
    }
}
