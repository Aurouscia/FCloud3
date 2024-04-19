using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Files.Storage.Abstractions
{
    public interface IFileStreamHasher
    {
        public string Hash(Stream s);
        public string Hash(MemoryStream s);
    }
}
