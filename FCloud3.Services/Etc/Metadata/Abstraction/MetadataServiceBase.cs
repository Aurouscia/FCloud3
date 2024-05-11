using FCloud3.Entities;
using FCloud3.Repos;
using Microsoft.Extensions.Logging;

namespace FCloud3.Services.Etc.Metadata.Abstraction
{
    public abstract class MetadataServiceBase<TMeta, TModel>
        where TMeta: MetadataBase<TModel> where TModel: class, IDbModel
    {
        protected readonly RepoBase<TModel> _repo;
        private readonly ILogger<MetadataServiceBase<TMeta, TModel>> _logger;

        /// <summary>
        /// 踏马的，这个List里面会莫名其妙出现null元素，
        /// 但是一断点调试就没了，可能是某种线程冲突引起的，加lock即可
        /// </summary>
        public static List<TMeta> DataList { get; } = [];
        private readonly static object _locker = new object();
        public MetadataServiceBase(RepoBase<TModel> repo, ILogger<MetadataServiceBase<TMeta, TModel>> logger) 
        {
            _repo = repo;
            _logger = logger;
        }
        protected TMeta? DataListSearch(Func<TMeta, bool> match)
            => DataList.Find(x => match(x));
        protected List<TMeta> DataListSearchRange(Func<TMeta, bool> match)
            => DataList.FindAll(x => match(x));

        public TMeta? Get(int id)
        {
            lock (_locker)
            {
                var stored = DataListSearch(x => x.Id == id);
                if (stored is not null)
                    return stored;
                var w = GetFromDbModel(_repo.GetqById(id)).FirstOrDefault();
                _logger.LogDebug("从数据库获取单个[{type}](元数据缓存)", typeof(TMeta).Name);
                if (w is null)
                    return null;
                DataList.Add(w);
                return w;
            }
        }
        public List<TMeta> GetRange(IEnumerable<int> ids)
        {
            lock (_locker)
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
                var fill = GetFromDbModel(_repo.GetRangeByIds(notFound)).ToList();
                _logger.LogDebug("从数据库获取多个[{type}](元数据缓存)", typeof(TMeta).Name);
                DataList.AddRange(fill);
                found.AddRange(fill);
                return found;
            }
        }
        public void Create(TMeta meta)
        {
            DataList.Add(meta);
        }
        public void Update(int id, Action<TMeta> action)
        {
            var stored = DataListSearch(x => x.Id == id);
            if (stored is not null)
                action(stored);
        }
        public void UpdateRange(IEnumerable<int> ids, Action<TMeta> action)
        {
            var stored = DataListSearchRange(x => ids.Contains(x.Id));
            stored.ForEach(x => action(x));
        }
        public List<TMeta> GetAll()
        {
            lock (_locker)
            {
                int allCount = _repo.ExistingCount;
                DataList.RemoveAll(x => x is null);
                if (allCount == DataList.Count)
                    return DataList;
                else
                {
                    DataList.Clear();
                    DataList.AddRange(GetFromDbModel(_repo.Existing).ToList());
                    _logger.LogDebug("从数据库获取全部[{type}](元数据缓存)", typeof(TMeta).Name);
                }
                return DataList;
            }
        }
        public void Remove(int id)
        {
            DataList.RemoveAll(x => x.Id == id);
        }
        protected abstract IQueryable<TMeta> GetFromDbModel(IQueryable<TModel> dbModels);
    }
}
