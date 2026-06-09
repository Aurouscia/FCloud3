using FCloud3.Services.Files.Storage.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace FCloud3.Services.Test.TestSupport
{
    internal class FakeFileItemHash : IFileItemHash
    {
        public string Hash(byte[] data)
        {
            byte[] hashBytes = MD5.HashData(data);
            StringBuilder sb = new();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public string Hash(Stream s)
        {
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return Hash(ms.ToArray());
        }
    }
}
