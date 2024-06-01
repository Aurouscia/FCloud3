﻿using FCloud3.DbContexts;
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
        protected readonly IQueryable<TModel> _dbExistingQ;
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
        private static int AllCount { get; set; } = allCountDefaultVal;
        private const int allCountDefaultVal = -1;
        public CachingBase(FCloudContext context, ILogger<CachingBase<TCache, TModel>> logger)
        {
            _dbExistingQ = context.Set<TModel>().Where(x=>!x.Deleted);
            _logger = logger;
        }
        protected TCache? DataListSearch(Func<TCache, bool> match)
            => DataList.Find(x => match(x));
        protected List<TCache> DataListSearchRange(Func<TCache, bool> match)
            => DataList.FindAll(x => match(x));

        #region 查询
        public TCache? Get(int id)
        {
            lock (Locker)
            {
                var stored = DataListSearch(x => x.Id == id);
                if (stored is not null)
                    return stored;
                var w = GetFromDbModel(_dbExistingQ.Where(x=>x.Id == id)).FirstOrDefault();
                _logger.LogDebug("从数据库获取单个[{type}](元数据缓存)", typeof(TCache).Name);
                if (w is null)
                    return null;
                DataList.Add(w);
                return w;
            }
        }
        public List<TCache> GetRange(IEnumerable<int> ids)
        {
            lock (Locker)
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
                DataList.AddRange(fill);
                found.AddRange(fill);
                return found;
            }
        }
        public List<TCache> GetAll()
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
                    _logger.LogDebug("从数据库获取全部[{type}](元数据缓存)", typeof(TCache).Name);
                }
                return DataList;
            }
        }
        #endregion

        #region 新增
        internal void Insert(TCache cachingModel)
        {
            lock (Locker)
            {
                DataList.Add(cachingModel);
                CountIncre();
            }
        }
        internal void Insert(TModel model) 
            => Insert(GetFromDbModel(model));

        internal void InsertRange(List<TCache> cachingModels)
        {
            lock (Locker)
            {
                DataList.AddRange(cachingModels);
                CountIncre(cachingModels.Count);
            }
        }
        internal void InsertRange(List<TModel> models)
            => InsertRange(models.ConvertAll(GetFromDbModel));
        #endregion

        #region 更新
        internal void Update(int id, Action<TCache> action)
        {
            lock (Locker)
            {
                var stored = DataListSearch(x => x.Id == id);
                if (stored is not null)
                    action(stored);
            }
        }
        internal void UpdateRange(IEnumerable<int> ids, Action<TCache> action)
        {
            lock (Locker)
            {
                var stored = DataListSearchRange(x => ids.Contains(x.Id));
                stored.ForEach(x => action(x));
            }
        }
        internal void UpdateByDbModel(TModel model)
        {
            var cache = Get(model.Id);
            lock (Locker)
            {
                if(cache is not null)
                    MutateByDbModel(cache, model);   
            }
        }
        internal void UpdateRangeByDbModel(List<TModel> models)
        {
            var caches = GetRange(models.ConvertAll(x=>x.Id));
            lock (Locker)
            {
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
        internal void Remove(int id)
        {
            lock (Locker)
            {
                DataList.RemoveAll(x => x.Id == id);
                CountDecre();
            }
        }

        internal void RemoveRange(List<int> ids)
        {
            lock (Locker)
            {
                DataList.RemoveAll(x => ids.Contains(x.Id));
                CountDecre(ids.Count);
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
                AllCount = allCountDefaultVal;
                _logger.LogWarning("[{type}]缓存已被清空(元数据缓存)", typeof(TCache).Name);
            }
        }
        private void CountIncre(int by = 1)
        {
            if (AllCount == allCountDefaultVal)
                AllCount = _dbExistingQ.Count();
            else
                AllCount += by;
        }
        private void CountDecre(int by = 1)
        {
            if (AllCount == allCountDefaultVal)
                AllCount = _dbExistingQ.Count();
            else
                AllCount -= by;
        }

        protected abstract IQueryable<TCache> GetFromDbModel(IQueryable<TModel> dbModels);
        protected abstract TCache GetFromDbModel(TModel model);
        protected abstract void MutateByDbModel(TCache target, TModel from);
    }
}
