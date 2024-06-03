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
            _context.UpdateRange(intoBlackList);
            _context.UpdateRange(outOfBlackList);
            _context.AddRange(newObjs);
            _context.SaveChanges();
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
        private List<WikiTitleContain> CheckDuplicate(List<WikiTitleContain> list)
        {
            var distincted = list.DistinctBy(x => x.WikiId).ToList();
            if (distincted.Count == list.Count)
                return distincted;
            var redundancy = list.Except(distincted).ToList();
            _context.RemoveRange(redundancy);
            _context.SaveChanges();
            return distincted;
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
