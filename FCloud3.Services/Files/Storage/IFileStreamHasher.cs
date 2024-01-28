using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Files.Storage
{
    public interface IFileStreamHasher
    {
        public string Hash(Stream s);
        public string Hash(Stream s, out Stream originalData);
    }
}
