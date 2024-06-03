using FCloud3.DbContexts;
using FCloud3.Entities;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Etc.Caching.Abstraction
{
    /// <summary>
    /// 用于减少数据库查询的托管内存缓存及其相关操作<br/>
    /// 应该在确认数据库操作执行成功后再调用本类对象更新缓存
    /// </summary>
    /// <typeparam name="TCache">内存缓存模型</typeparam>
    /// <typeparam name="TModel">数据库模型</typeparam>
    public abstract class CachingBase<TCache, TModel>
        where TCache : CachingModelBase<TModel> where TModel : class, IDbModel
    {
        private readonly IQueryable<TModel> _dbExistingQ;
        private readonly ILogger<CachingBase<TCache, TModel>> _logger;

        /// <summary>
        /// 存储数据处（注意List线程不安全）
        /// </summary>
        private static List<TCache> DataList { get; } = [];
        /// <summary>
        /// 使用静态对象确保同一个list同时只能被一个线程操作<br/>
        /// 注意：泛型类中的静态属性，对于不同的类型参数的实现，有多个静态属性
        /// </summary>
        protected static object Locker { get; } = new object();
        protected static bool HoldingAll { get; private set; }
        
        public int QueriedTimes { get; protected set; }
        public int QueriedRows { get; protected set; }
        public CachingBase(FCloudContext context, ILogger<CachingBase<TCache, TModel>> logger)
        {
            _dbExistingQ = context.Set<TModel>().Where(x=>!x.Deleted);
            _logger = logger;
        }
        public CachingBase(IQueryable<TModel> source, ILogger<CachingBase<TCache, TModel>> logger)
        {
            _dbExistingQ = source;
            _logger = logger;
        }
        private TCache? DataListSearch(Func<TCache, bool> match)
            => DataList.Find(x => match(x));
        private List<TCache> DataListSearchRange(Func<TCache, bool> match)
            => DataList.FindAll(x => match(x));

        #region 查询
        public TCache? Get(int id, bool useLock = true)
        {
            var action = () =>
            {
                var stored = DataListSearch(x => x.Id == id);
                if (stored is not null)
                    return stored;
                var w = GetFromDbModel(_dbExistingQ.Where(x => x.Id == id)).FirstOrDefault();
                _logger.LogDebug("从数据库获取单个[{type}](元数据缓存)", typeof(TCache).Name);
                QueriedTimes++;
                if (w is null)
                    return null;
                QueriedRows++;
                DataList.Add(w);
                return w;
            };
            if (!useLock)
                return action();
            lock (Locker)
            {
                return action();
            }
        }
        public List<TCache> GetRange(IEnumerable<int> ids, bool useLock = true)
        {
            var action = () =>
            {
                var found = new List<TCache>();
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
                _logger.LogDebug("从数据库获取多个[{type}](元数据缓存)", typeof(TCache).Name);
                QueriedTimes++;
                QueriedRows += fill.Count;
                DataList.AddRange(fill);
                found.AddRange(fill);
                return found;
            };
            if (!useLock)
                return action();
            lock (Locker)
            {
                return action();
            }
        }
        public List<TCache> GetAll(bool useLock=true)
        {
            var action = () =>
            {
                if (HoldingAll)
                    return DataList;
                else
                {
                    var loadedIds = DataList.Select(x => x.Id).ToList();
                    var q = _dbExistingQ.Where(x => !loadedIds.Contains(x.Id));
                    var qres = GetFromDbModel(q).ToList();
                    QueriedTimes++;
                    QueriedRows += qres.Count;
                    DataList.AddRange(qres);
                    HoldingAll = true;
                    _logger.LogDebug("从数据库获取全部[{type}](元数据缓存)", typeof(TCache).Name);
                }
                return DataList;
            };
            if (!useLock)
                return action();
            lock (Locker)
            {
                return action();
            }
        }
        public TCache? GetCustom(Func<TCache, bool> matchCache, Func<IQueryable<TModel>, IQueryable<TModel>> matchDb)
        {
            lock (Locker)
            {
                var cached = DataListSearch(matchCache);
                if (cached is not null)
                    return cached;
                var dbModel = matchDb(_dbExistingQ).FirstOrDefault();
                QueriedRows++;
                QueriedTimes++;
                if (dbModel is not null)
                {
                    var cacheObj = GetFromDbModel(dbModel);
                    Insert(cacheObj, false);
                    return cacheObj;
                }
                return null;
            }
        }
        #endregion

        #region 新增
        public void Insert(TCache cachingModel, bool useLock = true)
        {
            var action = () =>
            {
                DataList.Add(cachingModel);
                HoldingAll = false;
            };
            if (!useLock)
                action();
            else
            {
                lock (Locker)
                {
                    action();
                }
            }
        }
        public void Insert(TModel model, bool useLock = true) 
            => Insert(GetFromDbModel(model), useLock);

        public void InsertRange(List<TCache> cachingModels)
        {
            lock (Locker)
            {
                DataList.AddRange(cachingModels);
                HoldingAll = false;
            }
        }
        public void InsertRange(List<TModel> models)
            => InsertRange(models.ConvertAll(GetFromDbModel));
        #endregion

        #region 更新
        public void Update(int id, Action<TCache> action)
        {
            lock (Locker)
            {
                var stored = DataListSearch(x => x.Id == id);
                if (stored is not null)
                    action(stored);
            }
        }
        public void UpdateRange(IEnumerable<int> ids, Action<TCache> action)
        {
            lock (Locker)
            {
                var stored = DataListSearchRange(x => ids.Contains(x.Id));
                stored.ForEach(x => action(x));
            }
        }
        public void UpdateByDbModel(TModel model)
        {
            lock (Locker)
            {
                var cache = Get(model.Id, false);
                if(cache is not null)
                    MutateByDbModel(cache, model);   
            }
        }
        public void UpdateRangeByDbModel(List<TModel> models)
        {
            lock (Locker)
            {
                var caches = GetRange(models.ConvertAll(x=>x.Id), false);
                foreach (var c in caches)
                {
                    var corresponding = models.Find(x => x.Id == c.Id);
                    if (corresponding is not null)
                        MutateByDbModel(c, corresponding);
                }
            }
        }
        #endregion

        #region 删除
        public void Remove(int id)
        {
            lock (Locker)
            {
                int by = DataList.RemoveAll(x => x.Id == id);
                HoldingAll = false;
            }
        }

        public void RemoveRange(List<int> ids)
        {
            lock (Locker)
            {
                DataList.RemoveAll(x => ids.Contains(x.Id));
                HoldingAll = false;
            }
        }
        #endregion
        
        
        /// <summary>
        /// 丢弃所有缓存，仅在紧急情况使用
        /// </summary>
        public void Clear()
        {
            lock(Locker)
            {
                DataList.Clear();
                HoldingAll = false;
                _logger.LogWarning("[{type}]缓存已被清空(元数据缓存)", typeof(TCache).Name);
            }
        }
        private void NumberChanged()
        {
            //原本实现：新增/删除对象时顺便改动总数量，以便“查询所有”判断目前是否拥有所有的缓存
            //但发现改动数量是线程不安全的，只能重置会“不知道”的状态重新查询
            HoldingAll = false;
        }

        protected abstract IQueryable<TCache> GetFromDbModel(IQueryable<TModel> dbModels);
        protected abstract TCache GetFromDbModel(TModel model);
        protected abstract void MutateByDbModel(TCache target, TModel from);
        protected List<TCache> TestingOnlyGetDataList() => DataList;
        protected bool TestingOnlyHoldingAll => HoldingAll;
    }
}
