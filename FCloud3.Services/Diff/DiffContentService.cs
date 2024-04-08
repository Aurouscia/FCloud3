using FCloud3.Diff.String;
using FCloud3.Entities.Diff;
using FCloud3.Repos.Diff;
using FCloud3.Repos.Identities;

namespace FCloud3.Services.Diff
{
    public class DiffContentService(
        DiffContentRepo contentDiffRepo,
        UserRepo userRepo)
    {
        private readonly DiffContentRepo _contentDiffRepo = contentDiffRepo;
        private readonly UserRepo _userRepo = userRepo;

        public bool MakeDiffForTextSection(int textSecId, string? original, string? modified, out string? errmsg)
        {
            var diffs = StringDiffSearch.Run(original, modified);
            if (diffs.Count == 0)
            {
                errmsg = null;
                return true;
            }
            int removed = diffs.RemovedChars();
            int added = diffs.AddedChars();
            DiffContent dc = new()
            {
                ObjectId = textSecId,
                DiffType = DiffContentType.TextSection,
                RemovedChars = removed,
                AddedChars = added,
            };
            var dcId = _contentDiffRepo.TryAddAndGetId(dc, out errmsg);
            if (errmsg is not null)
                return false;
            
            var dss = diffs.ConvertAll(x => new DiffSingle()
            {
                DiffContentId = dcId,
                Index = x.Index,
                Ori = x.Ori,
                New = x.New,
            });
            return _contentDiffRepo.AddRangeDiffSingle(dss, out errmsg);
        }

        public DiffHistoryResult DiffHistory(DiffContentType type, int objId)
        {
            var list = (
                from diff in _contentDiffRepo.GetDiffs(type, objId)
                from user in _userRepo.Existing
                where diff.CreatorUserId == user.Id
                orderby diff.Created descending
                select new
                {
                    Time = diff.Created,
                    UserId = diff.CreatorUserId,
                    UserName = user.Name,
                    Removed = diff.RemovedChars,
                    Added = diff.AddedChars,
                }).ToList();
            var res = new DiffHistoryResult();
            foreach ( var item in list )
            {
                res.Add(item.Time, item.UserId, item.UserName, item.Removed, item.Added);
            }
            return res;
        }

        public class DiffHistoryResult
        {
            public List<DiffHistoryResultItem> Items { get; set; } = [];
            public void Add(DateTime time, int uid, string uname, int removed, int added)
            {
                Items.Add(new(time, uid, uname, removed, added));
            }
            public class DiffHistoryResultItem(DateTime time, int uid, string uname, int removed, int added)
            {
                public string T { get; set; } = time.ToString("yy/MM/dd HH:mm:ss");
                public int UId { get; set; } = uid;
                public string UName { get; set; } = uname;
                public int R { get; set; } = removed;
                public int A { get; set; } = added;
            }
        }
    }
}
