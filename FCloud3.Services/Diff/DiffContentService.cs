using Aurouscia.TableEditor.Core;
using FCloud3.Diff.Display;
using FCloud3.Diff.Object;
using FCloud3.Diff.String;
using FCloud3.Entities.Diff;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Diff;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Diff
{
    public class DiffContentService(
        DiffContentRepo contentDiffRepo,
        UserRepo userRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo,
        WikiParaRepo wikiParaRepo,
        WikiItemRepo wikiItemRepo)
    {
        private readonly DiffContentRepo _contentDiffRepo = contentDiffRepo;
        private readonly UserRepo _userRepo = userRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;

        public int MakeDiff(int objId, DiffContentType type, string? original, string? modified, out string? errmsg)
        {
            var diffs = StringDiffSearch.Run(original, modified);
            if (diffs.Count == 0)
            {
                errmsg = null;
                return 0;
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
            var dcId = _contentDiffRepo.AddAndGetId(dc);
            var dss = diffs.ConvertAll(x => new DiffSingle()
            {
                DiffContentId = dcId,
                Index = x.Index,
                Ori = x.Ori,
                New = x.New,
            });
            if (_contentDiffRepo.AddRangeDiffSingle(dss, out errmsg))
                return Math.Max(removed, added);
            return 0;
        }

        public DiffContentHistoryResult? DiffHistory(DiffContentType type, int objId, out string? errmsg, bool needObjInfo = false)
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
                    Hidden = diff.Hidden,
                }).ToList();
            var res = new DiffContentHistoryResult();
            if (needObjInfo)
            {
                var info = GetCurrentInfo(type, objId);
                if (info.HasValue)
                {
                    res.WikiPathName = _wikiItemRepo.CachedItemById(info.Value.wikiId)?.UrlPathName;
                    res.WikiParaName = info.Value.paraName;
                    res.WikiParaId = info.Value.paraId;
                }
            }
            foreach ( var item in list )
            {
                res.Add(item.DiffId, item.Time, item.UserId, item.UserName, item.Removed, item.Added, item.Hidden);
            }
            errmsg = null;
            return res;
        }

        public DiffContentHistoryResult? DiffHistoryForWiki(string wikiPathName, out string? errmsg)
        {
            var paras = (
                from para in _wikiParaRepo.Existing
                from w in _wikiItemRepo.Existing
                where w.UrlPathName == wikiPathName && para.WikiItemId == w.Id
                select new { para.Type, para.ObjectId }).ToList();
            List<(DiffContentType type, int objId)> targets = [];
            foreach (var p in paras)
            {
                if (p.Type == WikiParaType.Text)
                    targets.Add((DiffContentType.TextSection, p.ObjectId));
                else if(p.Type == WikiParaType.Table)
                    targets.Add((DiffContentType.FreeTable, p.ObjectId));
            }
            return DiffHistoryForRange(targets, out errmsg);
        }
        
        public DiffContentHistoryResult? DiffHistoryForRange(List<(DiffContentType type, int objId)> targets, out string? errmsg)
        {
            var diffs = _contentDiffRepo.GetDiffsForRange(targets);
            var users = _userRepo.AllCachedItems();
            var list = (
                from diff in diffs
                from user in users
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
                    Hidden = diff.Hidden
                }).ToList();
            var res = new DiffContentHistoryResult();
            foreach ( var item in list )
            {
                res.Add(item.DiffId, item.Time, item.UserId, item.UserName, item.Removed, item.Added, item.Hidden);
            }
            errmsg = null;
            return res;
        }

        public DiffContentDetailResult? DiffDetail(int diffId, out string? errmsg,
            bool canViewHidden = false, bool exactRevertCount = false)
        {
            var diff = _contentDiffRepo.GetById(diffId);
            if (diff is null)
            {
                errmsg = "找不到指定改动";
                return null;
            }
            var type = diff.DiffType;
            var objId = diff.ObjectId;
            string? content = GetCurrentContent(type, objId);
            if (content is null)
            {
                errmsg = "找不到指定内容";
                return null;
            }
            List<char> contentChars = [.. content];
            var diffs = _contentDiffRepo
                .GetDiffs(type, objId)
                .OrderByDescending(x => x.Created)
                .Select(x => new{x.Id, x.Hidden}).ToList();
            var targetIdx = diffs.FindIndex(d => d.Id == diffId);
            var diffCount = diffs.Count;
            var shouldRevertCount = exactRevertCount ? targetIdx + 1 : ShouldRevertCount(diffCount, targetIdx + 1);
            diffs = diffs.GetRange(0, shouldRevertCount);
            var diffIds = diffs.ConvertAll(x => x.Id);
            var singles = (
                from s in _contentDiffRepo.DiffSingles
                where diffIds.Contains(s.DiffContentId)
                select s).ToList();

            DiffContentDetailResult res = new();
            foreach (var d in diffs)
            {
                if (d.Hidden && !canViewHidden)
                {
                    res.Items.Add(new(d.Id, true));
                }
                else
                {
                    var itsDiffSingles = singles.FindAll(x => x.DiffContentId == d.Id);
                    StringDiffCollection sdc = ConvertToStringDiffCollection(itsDiffSingles);
                    var disp = DiffDisplay.Make(contentChars, sdc, 10);
                    res.Items.Add(new(d.Id, disp.From, disp.To));
                }
            }
            errmsg = null;
            return res;
        }
        [Obsolete("未考虑Hidden改动")]
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

        public bool SetHidden(int id, bool hidden, out string? errmsg)
        {
            return _contentDiffRepo.SetHidden(id, hidden, out errmsg);
        }

        public List<DiffContentHistoryResult>? GetDiffHistoriesByDays(DateTime date, int days, out string? errmsg)
        {
            errmsg = null;
            if (days < 0)
            {
                errmsg = "天数不能为负数";
                return null;
            }

            // 计算日期范围（包含今天）
            var endDate = date.AddDays(1).Date; // 明天的开始，即今天结束
            var startDate = date.AddDays(-days).Date; // days天前的开始

            // 获取在此范围内的所有 DiffContent 的 ObjectId 和 DiffType，去重
            var distinctTargets = _contentDiffRepo.Existing
                .Where(x => x.Created >= startDate && x.Created < endDate)
                .Select(x => new { x.ObjectId, x.DiffType })
                .Distinct()
                .ToList();

            var results = new List<DiffContentHistoryResult>();
            foreach (var target in distinctTargets)
            {
                var history = DiffHistory(target.DiffType, target.ObjectId, out var historyErrmsg, needObjInfo: true);
                if (historyErrmsg is not null)
                {
                    errmsg = historyErrmsg;
                    return null;
                }
                if (history is not null)
                {
                    results.Add(history);
                }
            }

            return results;
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
        private (int wikiId, string? paraName, int paraId)? GetCurrentInfo(DiffContentType type, int objId)
        {
            WikiPara? p = null;
            string? name = null;
            int paraId = 0;
            if (type == DiffContentType.TextSection)
            {
                p = _wikiParaRepo.Existing.FirstOrDefault(x => x.Type == WikiParaType.Text && x.ObjectId == objId);
                name = _textSectionRepo.GetqById(objId).Select(x => x.Title).FirstOrDefault();
                paraId = p?.Id ?? 0;
            }
            else if (type == DiffContentType.FreeTable)
            {
                p = _wikiParaRepo.Existing.FirstOrDefault(x => x.Type == WikiParaType.Table && x.ObjectId == objId);
                name = _freeTableRepo.GetqById(objId).Select(x => x.Name).FirstOrDefault();
                paraId = p?.Id ?? 0;
            }
            else
                throw new NotImplementedException();
            return p is not null ? (p.WikiItemId, p.NameOverride ?? name, paraId) : null;
        }

        private static int ShouldRevertCount(int total, int requested)
        {
            if (total < 10)
            {
                return total;
            }
            return Math.Min(total, requested + 3);

            //while (true)
            //{
            //    var half = total / 2;
            //    if (half < requested)
            //        return total;
            //    total = half;
            //}
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
            public string? WikiPathName { get; set; }
            public string? WikiParaName { get; set; }
            public int WikiParaId { get; set; }
            public List<DiffContentHistoryResultItem> Items { get; set; } = [];
            public void Add(int id, DateTime time, int uid, string uname, int removed, int added, bool hidden)
            {
                Items.Add(new(id, time, uid, uname, removed, added, hidden));
            }
            public class DiffContentHistoryResultItem(
                int id, DateTime time, int uid, string uname, int removed, int added, bool hidden)
            {
                public int Id { get; set; } = id;
                public string T { get; set; } = time.ToString("yy/MM/dd HH:mm:ss");
                public int UId { get; set; } = uid;
                public string UName { get; set; } = uname;
                public int R { get; set; } = removed;
                public int A { get; set; } = added;
                public bool H { get; set; } = hidden;
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
            public DiffContentStepDisplay(int id, bool hidden) : this(id, [], [])
            {
                Id = id;
                Hidden = hidden;
            }
            public int Id { get; set; } = id;
            public bool Hidden { get; set; }
            public List<DiffDisplayFrag> From { get; set; } = from;
            public List<DiffDisplayFrag> To { get; set; } = to;
        }
    }
}
