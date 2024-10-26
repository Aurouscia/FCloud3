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
        /// �洢Cache�����б���̰߳�ȫ�ֵ䣬keyΪId
        /// </summary>
        private static ConcurrentDictionary<int, TCache> CacheDict = [];
        private static DateTime LatestUpdated
            => CacheDict.Values.Select(x => x.Updated).Max();
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
        /// TODO: ���ݿ��Updated�ֶα���ÿ�θ������ݶ���Ϊ��ʱʱ�䣬���ܲ��裬�������ɻ��治ͬ��
        /// </summary>
        public DateTime Updated { get; } = updated;
    }
}