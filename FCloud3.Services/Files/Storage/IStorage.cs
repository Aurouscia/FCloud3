using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Files.Storage
{
    public interface IStorage
    {
        public bool Save(Stream s, string pathName, out string? errmsg);
        public bool Delete(string pathName, out string? errmsg);
        public string FullUrl(string pathName);
    }
}
