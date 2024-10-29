using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Wiki
{
    public class WikiTitleContainRepo : RepoBase<WikiTitleContain>
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
        
        public WikiTitleContainType ParaType2ContainType(WikiParaType wikiParaType)
        {
            if (wikiParaType == WikiParaType.Text)
                return WikiTitleContainType.TextSection;
            else if (wikiParaType == WikiParaType.Table)
                return WikiTitleContainType.FreeTable;
            return WikiTitleContainType.Unknown;
        }
        public WikiParaType ContainType2ParaType(WikiTitleContainType wikiTitleContainType)
        {
            if (wikiTitleContainType == WikiTitleContainType.TextSection)
                return WikiParaType.Text;
            else if (wikiTitleContainType == WikiTitleContainType.FreeTable)
                return WikiParaType.Table;
            throw new NotImplementedException();
        }
    }

    public static class WikiTitleContainQueryableExtension
    {
        public static IQueryable<WikiTitleContain> WithTypeAndId
            (this IQueryable<WikiTitleContain> q, WikiTitleContainType type, int objId)
            => q.Where(x => x.Type == type && x.ObjectId == objId);
        public static IQueryable<WikiTitleContain> WithTypeAndIds
            (this IQueryable<WikiTitleContain> q, WikiTitleContainType type, List<int> objIds)
            => q.Where(x => x.Type == type && objIds.Contains(x.ObjectId));
        public static IQueryable<WikiTitleContain> Siblings
            (this IQueryable<WikiTitleContain> q, WikiTitleContain with)
            => q.Where(x => x.Type == with.Type && x.ObjectId == with.ObjectId);
    }
}
