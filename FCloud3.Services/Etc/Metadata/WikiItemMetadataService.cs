using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc.Metadata.Abstraction;
using Microsoft.Extensions.Logging;

namespace FCloud3.Services.Etc.Metadata
{
    /// <summary>
    /// 获取和缓存词条对象的元数据，有更改必须在存入数据库时向其汇报
    /// </summary>
    public class WikiItemMetadataService(
        WikiItemRepo wikiItemRepo,
        ILogger<MetadataServiceBase<WikiItemMetadata, WikiItem>> logger) 
        : MetadataServiceBase<WikiItemMetadata, WikiItem>(wikiItemRepo, logger)
    {
        public WikiItemMetadata? Get(string urlPathName)
        {
            var stored = DataListSearch(x => x.UrlPathName == urlPathName);
            if (stored is not null)
                return stored;
            var w = GetFromDbModel(_repo.Existing.Where(x => x.UrlPathName == urlPathName))
                .FirstOrDefault();
            if (w is null) return null;
            DataList.Add(w);
            return w;
        }

        public void Create(int id, int ownerId, string title, string urlPathName)
        {
            WikiItemMetadata w = new(id, ownerId, title, urlPathName, DateTime.Now);
            base.Create(w);
        }

        protected override IQueryable<WikiItemMetadata> GetFromDbModel(IQueryable<WikiItem> dbModels)
        {
            return dbModels.Select(x => new WikiItemMetadata(x.Id, x.OwnerUserId, x.Title, x.UrlPathName, x.Updated));
        }
    }

    public class WikiItemMetadata : MetadataBase<WikiItem>
    {
        public int OwnerId { get; set; }
        public string? Title { get; set; }
        public string? UrlPathName { get; set; }
        public DateTime Update { get; set; }
        public WikiItemMetadata(int id, int ownerId, string? title, string? urlPathName, DateTime update)
        {
            Id = id;
            OwnerId = ownerId;
            Title = title;
            UrlPathName = urlPathName;
            Update = update;
        }
    }
}
