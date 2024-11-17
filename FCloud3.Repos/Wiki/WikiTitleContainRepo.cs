using FCloud3.DbContexts;
using FCloud3.Entities.Sys;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using System.Linq;

namespace FCloud3.Repos.Wiki
{
    public class WikiTitleContainRepo
        : RepoBaseCache<WikiTitleContain, WikiTitleContainCacheModel>
    {
        public WikiTitleContainRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public IQueryable<WikiTitleContain> BlackListed => Existing.Where(x => x.BlackListed);
        public IQueryable<WikiTitleContain> NotBlackListed => Existing.Where(x => !x.BlackListed);

        public int SetStatus(WikiTitleContainType type, int objId, List<int> wikiIds)
        {
            var wikiIdsSet = wikiIds.ToHashSet();
            var all = CachedContains(type, objId, false);
            var allIds = all.Select(x => x.Id);
            var needRemove = all.Where(x => !x.BlackListed && !wikiIdsSet.Contains(x.WikiId)).Select(x => x.Id).ToList();
            var needRecover = all.Where(x => x.BlackListed && wikiIdsSet.Contains(x.WikiId)).Select(x => x.Id).ToList();
            var needAdd = wikiIdsSet.Except(allIds);
            var newObjs = needAdd.Select(x => new WikiTitleContain
            {
                WikiId = x,
                Type = type,
                ObjectId = objId,
            }).ToList();
            var needUpdateIds = needRemove.Union(needRecover).ToList();
            if (needUpdateIds.Count > 0)
            {
                base.UpdateRangeByDelegateLocally(
                    x => needUpdateIds.Contains(x.Id),
                    x =>
                    {
                        if (needRemove.Contains(x.Id))
                            x.BlackListed = true;
                        else
                            x.BlackListed = false;
                    });
            }
            if(newObjs.Count > 0)
                base.AddRange(newObjs);
            return newObjs.Count + needUpdateIds.Count;
        }
        public void AppendForGroups(List<(WikiTitleContainType type, int objId, List<int> wikiIds)> groups)
        {
            var objs = new List<WikiTitleContain>();
            groups.ForEach(g => 
            {
                List<int> existingWikiIds = CachedContains(g.type, g.objId, false)
                    .Select(x => x.WikiId).ToList();
                g.wikiIds.ForEach(wId =>
                {
                    //排除已有的，只保留没有的
                    if (!existingWikiIds.Contains(wId))
                    {
                        objs.Add(new WikiTitleContain()
                        {
                            WikiId = wId,
                            Type = g.type,
                            ObjectId = g.objId,
                            BlackListed = false
                        });
                    }
                });
            });
            base.AddRange(objs);
        }

        public List<WikiTitleContain> GetByTypeAndObjId(WikiTitleContainType type, int objId, bool noBlackList = true)
        {
            var from = noBlackList ? NotBlackListed : All;
            return from.WithTypeAndId(type, objId).ToList();
        }
        public List<WikiTitleContain> GetByTypeAndObjIds(WikiTitleContainType type, List<int> objIds, bool noBlackList = true) 
        {
            //TODO：可以读缓存
            var from = noBlackList ? NotBlackListed : All;
            return from.WithTypeAndIds(type, objIds).ToList();
        }
        public List<WikiTitleContain> GetByTypeAndObjIds(WikiParaType type, List<int> objIds, bool noBlackList = true)
        {
            return GetByTypeAndObjIds(ParaType2ContainType(type), objIds, noBlackList);
        }
        public IEnumerable<WikiTitleContainCacheModel> GetByContaining(int wikiId)
        {
            return CachedItemsByPred(x => !x.BlackListed && x.WikiId == wikiId);
        }
        public IEnumerable<(WikiParaType Type, int ObjectId)> GetWikiParasByContaining(int wikiId)
        {
            var models = GetByContaining(wikiId);
            return models.Select(x => (ContainType2ParaType(x.Type), x.ObjId));
        }
        public IEnumerable<WikiTitleContainCacheModel> GetByWikiParas(
            List<(WikiParaType Type, int ObjectId)> wikiParas)
        {
            var tableIds = wikiParas
                .Where(x => ParaType2ContainType(x.Type) == WikiTitleContainType.FreeTable)
                .Select(x => x.ObjectId).ToList();
            var textIds = wikiParas
                .Where(x => ParaType2ContainType(x.Type) == WikiTitleContainType.TextSection)
                .Select(x => x.ObjectId).ToList();
            return CachedItemsByPred(c =>
                (c.Type == WikiTitleContainType.TextSection && textIds.Contains(c.ObjId)) ||
                (c.Type == WikiTitleContainType.FreeTable && tableIds.Contains(c.ObjId))
            );
        }

        public WikiTitleContainType ParaType2ContainType(WikiParaType wikiParaType)
        {
            if (wikiParaType == WikiParaType.Text)
                return WikiTitleContainType.TextSection;
            else if (wikiParaType == WikiParaType.Table)
                return WikiTitleContainType.FreeTable;
            throw new NotImplementedException();
        }
        public WikiParaType ContainType2ParaType(WikiTitleContainType wikiTitleContainType)
        {
            if (wikiTitleContainType == WikiTitleContainType.TextSection)
                return WikiParaType.Text;
            else if (wikiTitleContainType == WikiTitleContainType.FreeTable)
                return WikiParaType.Table;
            throw new NotImplementedException();
        }

        public IEnumerable<WikiTitleContainCacheModel> CachedContains(
            WikiTitleContainType type, int objId, bool noBlackList = true)
        {
            var res = CachedItemsByPred(x => x.ObjId == objId && x.Type == type);
            if (noBlackList)
                res = res.Where(x => !x.BlackListed);
            return res;
        }

        protected override IQueryable<WikiTitleContainCacheModel> ConvertToCacheModel(IQueryable<WikiTitleContain> q)
        {
            return q.Select(x => new WikiTitleContainCacheModel(
                x.Id, x.Updated, x.Type, x.ObjectId, x.WikiId, x.BlackListed));
        }

        protected override LastUpdateType GetLastUpdateType()
            => LastUpdateType.WikiTitleContain;
    }

    public class WikiTitleContainCacheModel(
        int id, DateTime updated, WikiTitleContainType type, int objId, int wikiId, bool blackListed)
        : CacheModelBase<WikiTitleContain>(id, updated)
    {
        public WikiTitleContainType Type { get; } = type;
        public int ObjId { get; } = objId;
        public int WikiId { get; } = wikiId;
        public bool BlackListed { get; } = blackListed;
    }

    public static class WikiTitleContainQueryableExtension
    {
        public static IQueryable<WikiTitleContain> WithTypeAndId
            (this IQueryable<WikiTitleContain> q, WikiTitleContainType type, int objId)
            => q.Where(x => x.Type == type && x.ObjectId == objId);
        public static IQueryable<WikiTitleContain> WithTypeAndIds
            (this IQueryable<WikiTitleContain> q, WikiTitleContainType type, List<int> objIds)
            => q.Where(x => x.Type == type && objIds.Contains(x.ObjectId));
    }
}
