using FCloud3.App.Utils;
using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.App.Services.Utils
{
    public class FileStreamHasher : IFileStreamHasher
    {
        public string Hash(Stream s)
        {
            return MD5Helper.GetMD5Of(s);
        }

        //TODO严重性能瓶颈
        //算多大的文件就要占走多少内存
        public string Hash(Stream s, out Stream originalData)
        {
            MemoryStream ms = new();
            s.CopyTo(ms);
            s.Flush();
            s.Close();
            ms.Position = 0;
            var hash = Hash(ms);
            ms.Position = 0;
            originalData = ms;
            return hash;
        }
    }
}
