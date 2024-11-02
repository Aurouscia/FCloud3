using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Sys;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Sys;
using System.Collections.Concurrent;

namespace FCloud3.Repos
{
    public abstract class RepoBaseCache<T, TCache>(
        FCloudContext context,
        LastUpdateRepo lastUpdateRepo,
        ICommitingUserIdProvider userIdProvider)
        : RepoBase<T>(context, userIdProvider)
        where T : class, IDbModel
        where TCache : CacheModelBase<T>
    {
        /// <summary>
        /// 存储Cache对象列表的线程安全字典，key为Id
        /// </summary>
        private static ConcurrentDictionary<int, TCache> CacheDict = [];
        private static DateTime LatestUpdatedInDict
        {
            get
            {
                if (CacheDict.IsEmpty)
                    return DateTime.MinValue;
                return CacheDict.Values.Select(x => x.Updated).Max();
            }
        }
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
        public List<TProp> CachedItemsPropByIds<TProp>(
            List<int> ids, Func<TCache, TProp> propSelector)
        {
            SynchronizeDictIfNecessary();
            List<TProp> res = new(ids.Count);
            foreach (var id in ids)
            {
                CacheDict.TryGetValue(id, out var item);
                if (item is { })
                    res.Add(propSelector(item));
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
                var latestUpdatedInDict = LatestUpdatedInDict;
                var lastUpdateInDb = GetLastUpdateInLuTable();
                //先判断数据库上次更新时间是否比Dict中更新
                if (lastUpdateInDb > latestUpdatedInDict || lastUpdateInDb == default)
                {    
                    if (!CacheDict.IsEmpty)
                    {
                        //如果已经有缓存，但还是需要进行同步，此时需要注意是否有新删掉的东西
                        //如果没有缓存，就不需要做这一步，直接取所有Existing即可
                        var delQ = Deleted.Where(x => x.Updated > latestUpdatedInDict);
                        var delIds = delQ.Select(x => x.Id).ToList();
                        delIds.ForEach(delId =>
                        {
                            CacheDict.TryRemove(delId, out _);
                        });
                    }
                    var q = Existing.Where(x => x.Updated > latestUpdatedInDict);
                    var fetched = ConvertToCacheModel(q).ToList();
                    fetched.ForEach(item =>
                    {
                        CacheDict.AddOrUpdate(item.Id, item, (id, oldVal) => item);
                    });
                    RepoCacheDictSyncFetchedRows += fetched.Count;
                    RepoCacheDictSyncTimes += 1;
                }
                Synchronized = true;
            }
        }


        protected override void Add(T item, DateTime? time = null)
        {
            var t = time ?? DateTime.Now;
            SetLastUpdateInLuTable(t);
            base.Add(item, t);
        }
        protected override void AddRange(List<T> items, DateTime? time = null)
        {
            var t = time ?? DateTime.Now;
            SetLastUpdateInLuTable(t);
            base.AddRange(items, t);
        }
        protected override void Update(T item, DateTime? time = null)
        {
            var t = time ?? DateTime.Now;
            SetLastUpdateInLuTable(t);
            base.Update(item, t);
        }
        protected override void UpdateRange(List<T> items, DateTime? time = null)
        {
            var t = time ?? DateTime.Now;
            SetLastUpdateInLuTable(t);
            base.UpdateRange(items, t);
        }
        protected override void Remove(int id, DateTime? time = null)
        {
            var t = time ?? DateTime.Now;
            SetLastUpdateInLuTable(t);
            base.Remove(id, time);
        }
        protected override void Remove(T item, DateTime? time = null)
        {
            var t = time ?? DateTime.Now;
            SetLastUpdateInLuTable(t);
            base.Remove(item, time);
        }
        protected override void RemoveRange(List<T> items, DateTime? time = null)
        {
            var t = time ?? DateTime.Now;
            SetLastUpdateInLuTable(t);
            base.RemoveRange(items, time);
        }

        public void UpdateTimeAndLu(int id)
        {
            var time = DateTime.Now;
            SetLastUpdateInLuTable(time);
            base.UpdateTime(id, time);
        }
        public void UpdateTimeAndLu(List<int> ids)
        {
            var time = DateTime.Now;
            SetLastUpdateInLuTable(time);
            base.UpdateTime(ids, time);
        }
        public int UpdateTimeAndLu(IQueryable<int> ids)
        {
            var time = DateTime.Now;
            SetLastUpdateInLuTable(time);
            return base.UpdateTime(ids, time);
        }
        protected void SetLastUpdateInLuTable(DateTime time)
        {
            var t = GetLastUpdateType();
            lastUpdateRepo.SetLastUpdateFor(t, time);
        }
        private DateTime GetLastUpdateInLuTable()
        {
            var t = GetLastUpdateType();
            return lastUpdateRepo.GetLastUpdateFor(t, GetLastUpdateInDbForInit);
        }
        protected abstract LastUpdateType GetLastUpdateType();
        private DateTime GetLastUpdateInDbForInit()
        {
            return Existing
                .OrderByDescending(x => x.Updated)
                .Select(x => x.Updated)
                .FirstOrDefault();
        }

        /// <summary>
        /// 清除缓存（仅在单元测试中使用（重置数据时），正常情况下不应使用）
        /// </summary>
        public void ClearCache()
        {
            CacheDict.Clear();
        }

        public int RepoCacheDictSyncTimes { get; private set; }
        public int RepoCacheDictSyncFetchedRows { get; private set; }

        /// <summary>
        /// 数据更改后，同一scope内的内存不再拥有一致性，如果需要读需要再次同步
        /// </summary>
        protected override void AfterDataChange()
        {
            Synchronized = false;
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