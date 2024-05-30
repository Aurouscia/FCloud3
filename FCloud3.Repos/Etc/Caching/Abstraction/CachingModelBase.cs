using FCloud3.Entities;

namespace FCloud3.Repos.Etc.Caching.Abstraction
{
    public abstract class CachingModelBase<T> where T : IDbModel
    {
        public int Id { get; set; }
    }
}
