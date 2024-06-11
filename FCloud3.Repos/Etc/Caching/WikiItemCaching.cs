using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using Microsoft.Extensions.Logging;
using FCloud3.Repos.Etc.Caching.Abstraction;
using FCloud3.DbContexts;

namespace FCloud3.Repos.Etc.Caching
{
    /// <summary>
    /// 获取和缓存词条对象的元数据，有更改必须在存入数据库时向其汇报
    /// </summary>
    public class WikiItemCaching(
        FCloudContext ctx,
        ILogger<CachingBase<WikiItemCachingModel, WikiItem>> logger)
        : CachingBase<WikiItemCachingModel, WikiItem>(ctx, logger)
    {
        public WikiItemCachingModel? Get(string urlPathName)
        {
            return GetCustom(
                x => x.UrlPathName == urlPathName,
                x => x.Where(w => w.UrlPathName == urlPathName));
        }

        protected override IQueryable<WikiItemCachingModel> GetFromDbModel(IQueryable<WikiItem> dbModels)
        {
            return dbModels.Select(x => new WikiItemCachingModel(x.Id, x.Sealed, x.OwnerUserId, x.Title, x.UrlPathName, x.Updated));
        }
        protected override WikiItemCachingModel GetFromDbModel(WikiItem model)
        {
            return new(model.Id, model.Sealed, model.OwnerUserId, model.Title, model.UrlPathName, model.Updated);
        }
        protected override void MutateByDbModel(WikiItemCachingModel target, WikiItem from)
        {
            target.Title = from.Title;
            target.Sealed = from.Sealed;
            target.UrlPathName = from.UrlPathName;
            target.Update = from.Updated;
            target.OwnerId = from.OwnerUserId;
        }
    }

    public class WikiItemCachingModel : CachingModelBase<WikiItem>
    {
        public int OwnerId { get; set; }
        public bool Sealed { get; set; }
        public string? Title { get; set; }
        public string? UrlPathName { get; set; }
        public DateTime Update { get; set; }
        public WikiItemCachingModel(int id, bool @sealed, int ownerId, string? title, string? urlPathName, DateTime update)
        {
            Id = id;
            Sealed = @sealed;
            OwnerId = ownerId;
            Title = title;
            UrlPathName = urlPathName;
            Update = update;
        }
    }
}
