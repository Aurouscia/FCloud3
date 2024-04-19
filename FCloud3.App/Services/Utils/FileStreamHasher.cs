using FCloud3.App.Utils;
using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.App.Services.Utils
{
    public class FileStreamHasher : IFileStreamHasher
    {
        public string Hash(Stream s)
        {
            return s.GetMD5();
        }

        public string Hash(MemoryStream s)
        {
            s.Seek(0, SeekOrigin.Begin);
            var hash = Hash(s as Stream);
            s.Seek(0, SeekOrigin.Begin);
            return hash;
        }
    }
}
