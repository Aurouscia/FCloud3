using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;

namespace FCloud3.Repos.Wiki
{
    public class WikiTitleContainRepo : RepoBase<WikiTitleContain>
    {
        public WikiTitleContainRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public List<WikiTitleContain> GetByTypeAndObjId(WikiTitleContainType type, int objId, bool onlyExisting = true)
        {
            var from = onlyExisting ? Existing : All;
            return from.Where(x=>x.Type == type && x.ObjectId == objId).ToList();
        }
        public List<WikiTitleContain> GetByTypeAndObjIds(WikiTitleContainType type, List<int> objIds, bool onlyExisting = true) 
        {
            var from = onlyExisting ? Existing : All;
            var res = from.Where(x => x.Type == type && objIds.Contains(x.ObjectId)).ToList();
            return res.DistinctBy(x=>x.WikiId).ToList();
        }
        public List<int> GetIdsByTypeAndObjIds(WikiTitleContainType type, List<int> objIds)
        {
            var res = All.Where(x => x.Type == type && objIds.Contains(x.ObjectId)).Select(x=>x.WikiId).ToList();
            return res.Distinct().ToList();
        }
    }
}
