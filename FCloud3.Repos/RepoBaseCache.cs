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
        /// �洢Cache�����б���̰߳�ȫ�ֵ䣬keyΪId
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
        /// Repo����Ӧ����Scoped����ÿ��Scope��һ�ε���ʱͬ��һ�μ��ɣ��˴���¼�Ƿ�ͬ����
        /// </summary>
        private bool Synchronized { get; set; } = false;
        /// <summary>
        /// �����ݿ�ģ��IQueryableӳ��ΪCacheģ��IQueryable
        /// </summary>
        /// <param name="q">���ݿ��ѯ����</param>
        /// <returns>Selectת�����</returns>
        protected abstract IQueryable<TCache> ConvertToCacheModel(IQueryable<T> q);
        /// <summary>
        /// ��ȡ����Cache���󣨽���ֻ��������ҪToList��������������
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
                //���ж����ݿ��ϴθ���ʱ���Ƿ��Dict�и���
                if (lastUpdateInDb > latestUpdatedInDict || lastUpdateInDb == default)
                {    
                    if (!CacheDict.IsEmpty)
                    {
                        //����Ѿ��л��棬��������Ҫ����ͬ������ʱ��Ҫע���Ƿ�����ɾ���Ķ���
                        //���û�л��棬�Ͳ���Ҫ����һ����ֱ��ȡ����Existing����
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
        /// ������棨���ڵ�Ԫ������ʹ�ã���������ʱ������������²�Ӧʹ�ã�
        /// </summary>
        public void ClearCache()
        {
            CacheDict.Clear();
        }

        public int RepoCacheDictSyncTimes { get; private set; }
        public int RepoCacheDictSyncFetchedRows { get; private set; }

        /// <summary>
        /// ���ݸ��ĺ�ͬһscope�ڵ��ڴ治��ӵ��һ���ԣ������Ҫ����Ҫ�ٴ�ͬ��
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
        /// TODO: ���ݿ��Updated�ֶα���ÿ�θ������ݶ���Ϊ��ʱʱ�䣬���ܲ��裬�������ɻ��治ͬ��
        /// </summary>
        public DateTime Updated { get; } = updated;
    }
}