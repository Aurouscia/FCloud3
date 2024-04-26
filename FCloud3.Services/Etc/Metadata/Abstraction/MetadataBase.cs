using FCloud3.Entities;

namespace FCloud3.Services.Etc.Metadata.Abstraction
{
    public abstract class MetadataBase<T> where T : IDbModel
    {
        public int Id { get; set; }
    }
}
