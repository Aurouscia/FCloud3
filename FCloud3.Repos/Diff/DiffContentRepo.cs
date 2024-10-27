using FCloud3.DbContexts;
using FCloud3.Entities.Diff;
using FCloud3.Repos.Etc;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos.Diff
{
    public class DiffContentRepo : RepoBase<DiffContent>
    {
        public DbSet<DiffSingle> DiffSingles { get; }
        public DiffContentRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
            DiffSingles = context.Set<DiffSingle>(); //代管该模型
        }

        public bool AddRangeDiffSingle(List<DiffSingle> diffSingles, out string? errmsg)
        {
            DiffSingles.AddRange(diffSingles);
            base.SaveChanges();
            errmsg = null;  
            return true;
        }

        public IQueryable<DiffContent> GetDiffs(DiffContentType type, int objId)
            => Existing.Where(x => x.DiffType == type && x.ObjectId == objId);

        public List<DiffContent> GetDiffsForRange(List<(DiffContentType type, int objId)> targets)
        {
            var objIds = targets.ConvertAll(x => x.objId);
            var cands = Existing.Where(x => objIds.Contains(x.ObjectId)).ToList();
            List<DiffContent> res = [];
            targets.ForEach(t =>
            {
                var items = cands.FindAll(x => x.DiffType == t.type && x.ObjectId == t.objId);
                res.AddRange(items);
            });
            return res;
        }

        public bool SetHidden(int id, bool hidden, out string? errmsg)
        {
            var affected = Existing
                .Where(dc => dc.Id == id)
                .ExecuteUpdate(spc => spc.SetProperty(dc => dc.Hidden, hidden));
            if (affected == 0)
            {
                errmsg = "找不到指定记录";
                return false;
            }
            errmsg = null;
            return true;
        }
    }
}
