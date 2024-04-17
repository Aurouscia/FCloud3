using Aurouscia.TableEditor.Core;
using FCloud3.Diff.Display;
using FCloud3.Diff.Object;
using FCloud3.Diff.String;
using FCloud3.Entities.Diff;
using FCloud3.Repos.Diff;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;

namespace FCloud3.Services.Diff
{
    public class DiffContentService(
        DiffContentRepo contentDiffRepo,
        UserRepo userRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo)
    {
        private readonly DiffContentRepo _contentDiffRepo = contentDiffRepo;
        private readonly UserRepo _userRepo = userRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;

        public bool MakeDiff(int objId, DiffContentType type, string? original, string? modified, out string? errmsg)
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
                ObjectId = objId,
                DiffType = type,
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

        public DiffContentDetailResult DiffDetail(DiffContentType type, int objId, int diffId)
        {
            string content = GetCurrentContent(type, objId) 
                ?? throw new Exception("找不到指定内容");
            List<char> contentChars = [.. content];
            var diffIds = _contentDiffRepo
                .GetDiffs(type, objId)
                .OrderByDescending(x=>x.Created)
                .Select(x=>x.Id).ToList();
            var targetIdx = diffIds.IndexOf(diffId);
            var diffCount = diffIds.Count;
            var shouldRevertCount = ShouldRevertCount(diffCount, targetIdx + 1);
            diffIds = diffIds.GetRange(0, shouldRevertCount);
            var singles = (
                from s in _contentDiffRepo.DiffSingles
                where diffIds.Contains(s.DiffContentId)
                select s).ToList();

            DiffContentDetailResult res = new();
            foreach(int diff in diffIds)
            {
                var itsDiffSingles = singles.FindAll(x=>x.DiffContentId == diff);
                StringDiffCollection sdc = ConvertToStringDiffCollection(itsDiffSingles);
                var disp = DiffDisplay.Make(contentChars, sdc, 10);
                res.Items.Add(new(diff, disp.From, disp.To));
            }
            return res;
        }
        public DiffContentCompleteResult DiffComplete(DiffContentType type, int objId, int diffId)
        {
            string content = GetCurrentContent(type, objId)
                ?? throw new Exception("找不到指定内容");
            List<char> contentChars = [.. content];
            var diffIds = _contentDiffRepo
                .GetDiffs(type, objId)
                .OrderByDescending(x => x.Created)
                .Select(x => x.Id).ToList();
            var targetIdx = diffIds.IndexOf(diffId);
            if(targetIdx == -1)
                throw new Exception("找不到指定改动点");
            diffIds = diffIds.GetRange(0, targetIdx + 1);
            var singles = (
                from s in _contentDiffRepo.DiffSingles
                where diffIds.Contains(s.DiffContentId)
                select s).ToList();

            DiffContentCompleteResult? res = null;
            foreach (int diff in diffIds)
            {
                var itsDiffSingles = singles.FindAll(x => x.DiffContentId == diff);
                StringDiffCollection sdc = ConvertToStringDiffCollection(itsDiffSingles);
                if (diff == diffId)
                {
                    var disp = DiffDisplay.Make(contentChars, sdc, 10);
                    res = new(new(diffId, disp.From, disp.To));
                    break;
                }
                else
                {
                    sdc.RevertAll(contentChars);
                }
            }
            if (res is null)
                throw new Exception("更改浏览(完整)出错");
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
            else if(type == DiffContentType.FreeTable)
            {
                return _freeTableRepo.Existing
                    .Where(x=>x.Id==objId)
                    .Select(x=>x.Data)
                    .FirstOrDefault();
            }
            throw new NotImplementedException();
        }
        private static int ShouldRevertCount(int total, int requested)
        {
            if (requested < 10 || total < 10)
            {
                if (total < 10)
                    return total;
                else
                    return 10;
            }
            while (true)
            {
                var half = total / 2;
                if (half < requested)
                    return total;
                total = half;
            }
        }
        private static StringDiffCollection ConvertToStringDiffCollection(List<DiffSingle> diffs)
        {
            var sdc = new StringDiffCollection(diffs.Count);
            diffs.ForEach(d =>
            {
                sdc.Add(new(d.Index, d.Ori ?? "", d.New));
            });
            return sdc;
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
            public List<DiffContentStepDisplay> Items { get; set; } = [];
        }
        public class DiffContentCompleteResult(DiffContentStepDisplay display)
        {
            public DiffContentStepDisplay Display { get; set; } = display;
        }
        public class DiffContentStepDisplay(int id, List<DiffDisplayFrag> from, List<DiffDisplayFrag> to)
        {
            public int Id { get; set; } = id;
            public List<DiffDisplayFrag> From { get; set; } = from;
            public List<DiffDisplayFrag> To { get; set; } = to;
        }
    }
}
