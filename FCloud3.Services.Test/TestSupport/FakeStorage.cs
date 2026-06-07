using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.Services.Test.TestSupport
{
    public class FakeStorage : IStorage
    {
        public StorageProvideType ProvideType => StorageProvideType.Stream;

        private readonly Dictionary<string, byte[]> _stored = [];
        public const string TestUrlBase = "http://test-storage/";

        public bool Delete(string pathName, out string? errmsg)
        {
            _stored.Remove(pathName);
            errmsg = null;
            return true;
        }

        public string FullUrl(string pathName)
        {
            return TestUrlBase + pathName;
        }

        public string GetUrlBase()
        {
            return TestUrlBase;
        }

        public Stream? Read(string pathName)
        {
            if (_stored.TryGetValue(pathName, out var bytes))
                return new MemoryStream(bytes);
            return null;
        }

        public bool Save(Stream s, string pathName, out string? errmsg)
        {
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            _stored[pathName] = ms.ToArray();
            errmsg = null;
            return true;
        }

        public void StoreForTest(string pathName, byte[] data)
        {
            _stored[pathName] = data;
        }
    }
}
