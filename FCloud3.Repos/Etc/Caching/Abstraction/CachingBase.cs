using FCloud3.DbContexts;
using FCloud3.Entities;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Etc.Caching.Abstraction
{
    /// <summary>
    /// 用于减少数据库查询的托管内存缓存及其相关操作<br/>
    /// 应该在确认数据库操作执行成功后再调用本类对象更新缓存
    /// </summary>
    /// <typeparam name="TMeta">内存缓存模型</typeparam>
    /// <typeparam name="TModel">数据库模型</typeparam>
    public abstract class CachingBase<TMeta, TModel>
        where TMeta : CachingModelBase<TModel> where TModel : class, IDbModel
    {
        protected readonly IQueryable<TModel> _dbExistingQ;
        private readonly ILogger<CachingBase<TMeta, TModel>> _logger;

        /// <summary>
        /// 存储数据处（注意List线程不安全）
        /// </summary>
        private static List<TMeta> DataList { get; } = [];
        /// <summary>
        /// 使用静态对象确保同一个list同时只能被一个线程操作
        /// </summary>
        protected static object Locker { get; } = new object();
        private static int AllCount { get; set; } = allCountDefaultVal;
        private const int allCountDefaultVal = -1;
        public CachingBase(FCloudContext context, ILogger<CachingBase<TMeta, TModel>> logger)
        {
            _dbExistingQ = context.Set<TModel>().Where(x=>!x.Deleted);
            _logger = logger;
        }
        protected TMeta? DataListSearch(Func<TMeta, bool> match)
            => DataList.Find(x => match(x));
        protected List<TMeta> DataListSearchRange(Func<TMeta, bool> match)
            => DataList.FindAll(x => match(x));

        public TMeta? Get(int id)
        {
            lock (Locker)
            {
                var stored = DataListSearch(x => x.Id == id);
                if (stored is not null)
                    return stored;
                var w = GetFromDbModel(_dbExistingQ.Where(x=>x.Id == id)).FirstOrDefault();
                _logger.LogDebug("从数据库获取单个[{type}](元数据缓存)", typeof(TMeta).Name);
                if (w is null)
                    return null;
                DataList.Add(w);
                return w;
            }
        }
        public List<TMeta> GetRange(IEnumerable<int> ids)
        {
            lock (Locker)
            {
                var found = new List<TMeta>();
                var notFound = new List<int>();
                foreach (var id in ids)
                {
                    var stored = DataListSearch(x => x.Id == id);
                    if (stored is not null)
                        found.Add(stored);
                    else
                        notFound.Add(id);
                }
                if (notFound.Count == 0)
                    return found;
                var fill = GetFromDbModel(_dbExistingQ.Where(x => notFound.Contains(x.Id))).ToList();
                _logger.LogDebug("从数据库获取多个[{type}](元数据缓存)", typeof(TMeta).Name);
                DataList.AddRange(fill);
                found.AddRange(fill);
                return found;
            }
        }
        public void Insert(TMeta meta)
        {
            lock (Locker)
            {
                DataList.Add(meta);
                CountIncre();
            }
        }
        public void Update(int id, Action<TMeta> action)
        {
            lock (Locker)
            {
                var stored = DataListSearch(x => x.Id == id);
                if (stored is not null)
                    action(stored);
            }
        }
        public void UpdateRange(IEnumerable<int> ids, Action<TMeta> action)
        {
            lock (Locker)
            {
                var stored = DataListSearchRange(x => ids.Contains(x.Id));
                stored.ForEach(x => action(x));
            }
        }
        public List<TMeta> GetAll()
        {
            lock (Locker)
            {
                if (AllCount == DataList.Count)
                    return DataList;
                else
                {
                    var loadedIds = DataList.Select(x => x.Id).ToList();
                    var q = _dbExistingQ.Where(x => !loadedIds.Contains(x.Id));
                    DataList.AddRange(GetFromDbModel(q).ToList());
                    AllCount = DataList.Count;
                    _logger.LogDebug("从数据库获取全部[{type}](元数据缓存)", typeof(TMeta).Name);
                }
                return DataList;
            }
        }
        public void Remove(int id)
        {
            lock (Locker)
            {
                DataList.RemoveAll(x => x.Id == id);
                CountDecre();
            }
        }
        /// <summary>
        /// 丢弃所有缓存，仅在紧急情况使用
        /// </summary>
        public void Clear()
        {
            lock(Locker)
            {
                DataList.Clear();
            }
        }

        private void CountIncre()
        {
            if (AllCount == allCountDefaultVal)
                AllCount = _dbExistingQ.Count();
            else
                AllCount++;
        }
        private void CountDecre()
        {
            if (AllCount == allCountDefaultVal)
                AllCount = _dbExistingQ.Count();
            else
                AllCount--;
        }

        protected abstract IQueryable<TMeta> GetFromDbModel(IQueryable<TModel> dbModels);
    }
}
