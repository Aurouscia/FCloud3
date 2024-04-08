using FCloud3.Diff.String;
using FCloud3.Entities.Etc;
using FCloud3.Repos.Etc;

namespace FCloud3.Services.Etc
{
    public class DiffContentService
    {
        private readonly DiffContentRepo _contentDiffRepo;

        public DiffContentService(DiffContentRepo contentDiffRepo)
        {
            _contentDiffRepo = contentDiffRepo;
        }
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
                DiffType = ContentDiffType.TextSection,
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
    }
}
