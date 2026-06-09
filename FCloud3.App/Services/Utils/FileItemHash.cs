using FCloud3.App.Utils;
using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.App.Services.Utils
{
    public class FileItemHash : IFileItemHash
    {
        public string Hash(byte[] data)
        {
            using var ms = new MemoryStream(data);
            return ms.GetMD5();
        }

        public string Hash(Stream s)
        {
            return s.GetMD5();
        }
    }
}
