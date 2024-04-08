using FCloud3.DbContexts;
using FCloud3.Entities.Etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Etc
{
    public class DiffContentRepo : RepoBase<DiffContent>
    {
        public DiffContentRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public bool AddRangeDiffSingle(List<DiffSingle> diffSingles, out string? errmsg)
        {
            _context.DiffSingles.AddRange(diffSingles);
            _context.SaveChanges();
            errmsg = null;  
            return true;
        }
    }
}
