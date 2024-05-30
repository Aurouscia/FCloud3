using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using Microsoft.Extensions.Logging;
using FCloud3.Repos.Etc.Caching.Abstraction;

namespace FCloud3.Repos.Etc.Caching
{
    /// <summary>
    /// 获取和缓存词条对象的元数据，有更改必须在存入数据库时向其汇报
    /// </summary>
    public class WikiItemCaching(
        WikiItemRepo wikiItemRepo,
        ILogger<CachingBase<WikiItemCachingModel, WikiItem>> logger)
        : CachingBase<WikiItemCachingModel, WikiItem>(wikiItemRepo, logger)
    {
        public WikiItemCachingModel? Get(string urlPathName)
        {
            var stored = DataListSearch(x => x.UrlPathName == urlPathName);
            if (stored is not null)
                return stored;
            var w = GetFromDbModel(_repo.Existing.Where(x => x.UrlPathName == urlPathName))
                .FirstOrDefault();
            if (w is null) return null;
            Insert(w);
            return w;
        }

        public void Create(int id, int ownerId, string title, string urlPathName)
        {
            WikiItemCachingModel w = new(id, ownerId, title, urlPathName, DateTime.Now);
            Insert(w);
        }

        protected override IQueryable<WikiItemCachingModel> GetFromDbModel(IQueryable<WikiItem> dbModels)
        {
            return dbModels.Select(x => new WikiItemCachingModel(x.Id, x.OwnerUserId, x.Title, x.UrlPathName, x.Updated));
        }

        private static readonly object LockObj = new();
        protected override object Locker => LockObj;
    }

    public class WikiItemCachingModel : CachingModelBase<WikiItem>
    {
        public int OwnerId { get; set; }
        public string? Title { get; set; }
        public string? UrlPathName { get; set; }
        public DateTime Update { get; set; }
        public WikiItemCachingModel(int id, int ownerId, string? title, string? urlPathName, DateTime update)
        {
            Id = id;
            OwnerId = ownerId;
            Title = title;
            UrlPathName = urlPathName;
            Update = update;
        }
    }
}
