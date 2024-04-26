using FCloud3.Entities;
using FCloud3.Repos;

namespace FCloud3.Services.Etc.Metadata.Abstraction
{
    public abstract class MetadataServiceBase<TMeta, TModel>
        where TMeta: MetadataBase<TModel> where TModel: class, IDbModel
    {
        protected readonly RepoBase<TModel> _repo;
        public static List<TMeta> DataList { get; set; } = [];
        public MetadataServiceBase(RepoBase<TModel> repo) 
        {
            _repo = repo;
        }
        public TMeta? Get(int id)
        {
            var stored = DataList.FirstOrDefault(x => x.Id == id);
            if (stored is not null)
                return stored;
            var w = GetFromDbModel(_repo.GetqById(id)).FirstOrDefault();
            if (w is null) return null;
            DataList.Add(w);
            return w;
        }
        public void Create(TMeta meta)
        {
            DataList.Add(meta);
        }
        public void Update(int id, Action<TMeta> action)
        {
            var stored = DataList.FirstOrDefault(x => x.Id == id);
            if (stored is not null)
                action(stored);
        }
        public void UpdateRange(IEnumerable<int> ids, Action<TMeta> action)
        {
            var stored = DataList.FindAll(x => ids.Contains(x.Id));
            stored.ForEach(x => action(x));
        }
        public List<TMeta> GetAll()
        {
            int allCount = _repo.ExistingCount;
            if (allCount == DataList.Count)
                return DataList;
            else
                DataList = GetFromDbModel(_repo.Existing).ToList();
            return DataList;
        }
        protected abstract IQueryable<TMeta> GetFromDbModel(IQueryable<TModel> dbModels);
    }
}
