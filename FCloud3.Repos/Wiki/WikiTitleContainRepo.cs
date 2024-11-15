﻿using FCloud3.DbContexts;
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

        public bool SetStatus(
            List<WikiTitleContain> intoBlackList,
            List<WikiTitleContain> outOfBlackList,
            List<WikiTitleContain> newObjs,
            out string? errmsg)
        {
            intoBlackList.ForEach(x => x.BlackListed = true);
            outOfBlackList.ForEach(x => x.BlackListed = false);
            var needUpdate = new List<WikiTitleContain>(
                intoBlackList.Count + outOfBlackList.Count);
            needUpdate.AddRange(intoBlackList);
            needUpdate.AddRange(outOfBlackList);
            base.UpdateRange(intoBlackList);
            base.AddRange(newObjs);
            errmsg = null;
            return true;
        }

        public List<WikiTitleContain> GetByTypeAndObjId(WikiTitleContainType type, int objId, bool noBlackList = true)
        {
            var from = noBlackList ? NotBlackListed : All;
            var res = from.WithTypeAndId(type, objId).ToList();
            return CheckDuplicate(res);
        }
        public List<WikiTitleContain> GetByTypeAndObjIds(WikiTitleContainType type, List<int> objIds, bool noBlackList = true) 
        {
            //TODO：可以读缓存
            var from = noBlackList ? NotBlackListed : All;
            var res = from.WithTypeAndIds(type, objIds).ToList();
            return CheckDuplicate(res);
        }
        public List<WikiTitleContain> GetByTypeAndObjIds(WikiParaType type, List<int> objIds, bool noBlackList = true)
        {
            return GetByTypeAndObjIds(ParaType2ContainType(type), objIds, noBlackList);
        }
        private List<WikiTitleContain> CheckDuplicate(List<WikiTitleContain> list)
        {
            var distincted = list.DistinctBy(x =>
                x.WikiId.GetHashCode() + x.Type.GetHashCode() + x.ObjectId.GetHashCode()).ToList();
            if (distincted.Count == list.Count)
                return distincted;
            var redundancy = list.Except(distincted).ToList();
            base.RemoveRange(redundancy);
            return distincted;
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

        private IEnumerable<WikiTitleContainCacheModel> CachedContainAll(
            int objId, WikiTitleContainType type)
        {
            return CachedItemsByPred(x => x.ObjId == objId && x.Type == type);
        }
        private IEnumerable<WikiTitleContainCacheModel> CachedContain(
            bool blackListed, int objId, WikiTitleContainType type)
        {
            return CachedContainAll(objId, type).Where(x => x.BlackListed == blackListed);
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
