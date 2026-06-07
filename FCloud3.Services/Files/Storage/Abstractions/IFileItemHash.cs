namespace FCloud3.Services.Files.Storage.Abstractions
{
    public interface IFileItemHash
    {
        public string Hash(byte[] data);
        public string Hash(Stream s);
    }
}
