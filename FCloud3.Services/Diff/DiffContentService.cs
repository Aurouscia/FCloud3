using FCloud3.Diff.String;
using FCloud3.Entities.Diff;
using FCloud3.Repos.Diff;
using FCloud3.Repos.Identities;
using FCloud3.Repos.TextSec;

namespace FCloud3.Services.Diff
{
    public class DiffContentService(
        DiffContentRepo contentDiffRepo,
        UserRepo userRepo,
        TextSectionRepo textSectionRepo)
    {
        private readonly DiffContentRepo _contentDiffRepo = contentDiffRepo;
        private readonly UserRepo _userRepo = userRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;

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

        public DiffContentHistoryResult DiffHistory(DiffContentType type, int objId)
        {
            var list = (
                from diff in _contentDiffRepo.GetDiffs(type, objId)
                from user in _userRepo.Existing
                where diff.CreatorUserId == user.Id
                orderby diff.Created descending
                select new
                {
                    DiffId = diff.Id,
                    Time = diff.Created,
                    UserId = diff.CreatorUserId,
                    UserName = user.Name,
                    Removed = diff.RemovedChars,
                    Added = diff.AddedChars,
                }).ToList();
            var res = new DiffContentHistoryResult();
            foreach ( var item in list )
            {
                res.Add(item.DiffId, item.Time, item.UserId, item.UserName, item.Removed, item.Added);
            }
            return res;
        }

        public DiffContentDetailResult DiffDetail(DiffContentType type, int objId)
        {
            string content = GetCurrentContent(type, objId) 
                ?? throw new Exception("找不到指定内容");
            List<char> contentChars = [.. content];
            var diffs = _contentDiffRepo
                .GetDiffs(type, objId)
                .OrderByDescending(x=>x.Created)
                .Select(x=>x.Id).ToList();
            var diffSingles = _contentDiffRepo.DiffSingles
                .Where(x => diffs.Contains(x.DiffContentId))
                .ToList();
            DiffContentDetailResult res = new();

            res.AddItem(0, content, []);
            foreach (int id in diffs)
            {
                var last = res.Items.Last();
                var dsHere = diffSingles.FindAll(x => x.DiffContentId == id);
                StringDiffCollection stringDiffs = [];
                List<int[]> removed = [];
                dsHere.ForEach(x => {
                    stringDiffs.Add(new StringDiff(x.Index, x.Ori ?? "", x.New));
                    if(x.New > 0)
                        last.Added.Add([x.Index, x.Index + x.New]);
                    int oriLength = x.Ori is not null ? x.Ori.Length : 0;
                    if(oriLength > 0)
                        removed.Add([x.Index, x.Index + oriLength]);
                });
                stringDiffs.RevertAll(contentChars);
                string contentThere = new(contentChars.ToArray());
                res.AddItem(id, contentThere, removed);
            }
            return res;
        }

        private string? GetCurrentContent(DiffContentType type, int objId)
        {
            if(type == DiffContentType.TextSection)
            {
                return _textSectionRepo.Existing
                    .Where(x => x.Id == objId)
                    .Select(x => x.Content)
                    .FirstOrDefault();
            }
            throw new NotImplementedException();
        }

        public class DiffContentHistoryResult
        {
            public List<DiffContentHistoryResultItem> Items { get; set; } = [];
            public void Add(int id, DateTime time, int uid, string uname, int removed, int added)
            {
                Items.Add(new(id, time, uid, uname, removed, added));
            }
            public class DiffContentHistoryResultItem(int id, DateTime time, int uid, string uname, int removed, int added)
            {
                public int Id { get; set; } = id;
                public string T { get; set; } = time.ToString("yy/MM/dd HH:mm:ss");
                public int UId { get; set; } = uid;
                public string UName { get; set; } = uname;
                public int R { get; set; } = removed;
                public int A { get; set; } = added;
            }
        }

        public class DiffContentDetailResult
        {
            public List<DiffContentDetailResultItem> Items { get; set; } = [];
            public void AddItem(int id, string content, List<int[]> removed)
            {
                Items.Add(new(id, content, removed));
            }
            public class DiffContentDetailResultItem(int id, string content, List<int[]> removed)
            {
                public int Id { get; set; } = id;
                public string Content { get; set; } = content;
                public List<int[]> Added { get; set; } = [];
                public List<int[]> Removed { get; set; } = removed;
            }
        }
    }
}
