using FCloud3.DbContexts;
using FCloud3.Entities.Diff;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Diff
{
    public class DiffContentRepo : RepoBase<DiffContent>
    {
        public DiffContentRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public bool AddRangeDiffSingle(List<DiffSingle> diffSingles, out string? errmsg)
        {
            //DiffSingles实体并不实现IDbModel接口，情况特殊，需要直接操作_context
            _context.DiffSingles.AddRange(diffSingles);
            _context.SaveChanges();
            errmsg = null;  
            return true;
        }

        public IQueryable<DiffContent> GetDiffs(DiffContentType type, int objId)
            => Existing.Where(x => x.DiffType == type && x.ObjectId == objId);

        public IQueryable<DiffSingle> DiffSingles => _context.Set<DiffSingle>();
    }
}
