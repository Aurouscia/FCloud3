using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Files.Storage.Abstractions
{
    public interface IStorage
    {
        public bool Save(Stream s, string pathName, out string? errmsg);
        public bool Delete(string pathName, out string? errmsg);
        public string GetUrlBase();
        public string FullUrl(string pathName);
        public Stream? Read(string pathName);
        public StorageProvideType ProvideType { get; }
    }
    public enum StorageProvideType
    {
        Unknown,
        Stream,
        Redirect
    }
}
