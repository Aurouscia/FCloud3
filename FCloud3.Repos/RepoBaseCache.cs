using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Repos.Etc;
using System.Collections.Concurrent;

namespace FCloud3.Repos
{
    public abstract class RepoBaseCache<T, TCache>(
        FCloudContext context,
        ICommitingUserIdProvider userIdProvider) 
        : RepoBase<T>(context, userIdProvider) 
        where T : class, IDbModel 
        where TCache : CacheModelBase<T>
    {
        /// <summary>
        /// 存储Cache对象列表的线程安全字典，key为Id
        /// </summary>
        private static ConcurrentDictionary<int, TCache> CacheDict = [];
        private static DateTime LatestUpdated
            => CacheDict.Values.Select(x => x.Updated).Max();
        /// <summary>
        /// Repo对象应该是Scoped服务，每个Scope第一次调用时同步一次即可，此处记录是否同步过
        /// </summary>
        private bool Synchronized { get; set; } = false;
        /// <summary>
        /// 从数据库模型IQueryable映射为Cache模型IQueryable
        /// </summary>
        /// <param name="q">数据库查询对象</param>
        /// <returns>Select转换后的</returns>
        protected abstract IQueryable<TCache> ConvertToCacheModel(IQueryable<T> q);
        /// <summary>
        /// 获取所有Cache对象（建议只遍历，不要ToList产生大量垃圾）
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TCache> AllCachedItems()
        {
            SynchronizeDictIfNecessary();
            return CacheDict.Values;
        }
        public TCache? CachedItemById(int id)
        {
            SynchronizeDictIfNecessary();
            CacheDict.TryGetValue(id, out var item);
            return item;
        }
        public List<TCache> CachedItemsByIds(List<int> ids)
        {
            SynchronizeDictIfNecessary();
            List<TCache> res = new(ids.Count);
            foreach (var id in ids)
            {
                CacheDict.TryGetValue(id, out var item);
                if(item is { })
                    res.Add(item);
            }
            return res;
        }
        public IEnumerable<TCache> CachedItemsByPred(Func<TCache, bool> pred)
        {
            SynchronizeDictIfNecessary();
            return CacheDict.Values.Where(pred);
        }
        public TCache? CachedItemByPred(Func<TCache, bool> pred)
        {
            SynchronizeDictIfNecessary();
            return CacheDict.Values.Where(pred).FirstOrDefault();
        }
        public int CachedItemsCount()
            => CacheDict.Count;
        private void SynchronizeDictIfNecessary()
        {
            if (!Synchronized)
            {
                var latestUpdated = LatestUpdated;
                var q = Existing.Where(x => x.Updated > latestUpdated);
                var fetched = ConvertToCacheModel(q).ToList();
                fetched.ForEach(item =>
                {
                    CacheDict.AddOrUpdate(item.Id, item, (id, oldVal) => item);
                });
                Synchronized = true;
            }
        }
    }

    public abstract class CacheModelBase<T>(int id, DateTime updated) where T : IDbModel
    {
        public int Id { get; } = id;
        /// <summary>
        /// TODO: 数据库的Updated字段必须每次更新数据都设为当时时间，不能不设，否则会造成缓存不同步
        /// </summary>
        public DateTime Updated { get; } = updated;
    }
}