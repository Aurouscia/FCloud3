using FCloud3.DbContexts;
using FCloud3.Entities.Diff;
using FCloud3.Entities.TextSection;
using FCloud3.Repos.Diff;
using FCloud3.Services.Test.TestSupport;
using FCloud3.Services.TextSec;

namespace FCloud3.Services.Test.Diff
{
    [TestClass]
    public class TextSectionDiffTest
    {
        private readonly FCloudContext _ctx;
        private readonly TextSectionService _textSectionService;
        private readonly DiffContentRepo _diffContentRepo;

        public TextSectionDiffTest()
        {
            var provider = new TestingServiceProvider();
            _textSectionService = provider.Get<TextSectionService>();
            _diffContentRepo = provider.Get<DiffContentRepo>();
            _ctx = provider.Get<FCloudContext>();

            var ts = new TextSection
            {
                Title = "Test",
                Content = "Original content"
            };
            _ctx.TextSections.Add(ts);
            _ctx.SaveChanges();
            _ctx.ChangeTracker.Clear();
        }

        [TestMethod]
        public void UpdateContent_CreatesDiff()
        {
            var success = _textSectionService.TryUpdate(1, null, "Modified content", out var errmsg);
            Assert.IsTrue(success, errmsg);
            _ctx.ChangeTracker.Clear();

            var diffs = _diffContentRepo.GetDiffs(DiffContentType.TextSection, 1).ToList();
            Assert.AreEqual(1, diffs.Count);
            Assert.AreEqual(1, diffs[0].ObjectId);
            Assert.AreEqual(DiffContentType.TextSection, diffs[0].DiffType);
            Assert.AreEqual(8, diffs[0].RemovedChars); // "Original"
            Assert.AreEqual(8, diffs[0].AddedChars);   // "Modified"

            var singles = _diffContentRepo.DiffSingles
                .Where(x => x.DiffContentId == diffs[0].Id)
                .ToList();
            Assert.AreEqual(1, singles.Count);
            Assert.AreEqual(0, singles[0].Index);
            Assert.AreEqual("Original", singles[0].Ori);
            Assert.AreEqual(8, singles[0].New);
        }

        [TestMethod]
        public void UpdateContent_SameContent_NoDiff()
        {
            var success = _textSectionService.TryUpdate(1, null, "Original content", out var errmsg);
            Assert.IsTrue(success, errmsg);
            _ctx.ChangeTracker.Clear();

            var diffs = _diffContentRepo.GetDiffs(DiffContentType.TextSection, 1).ToList();
            Assert.AreEqual(0, diffs.Count);
        }

        [TestMethod]
        public void UpdateContent_MultipleTimes_CreatesMultipleDiffs()
        {
            var success1 = _textSectionService.TryUpdate(1, null, "First change", out var errmsg1);
            Assert.IsTrue(success1, errmsg1);
            _ctx.ChangeTracker.Clear();

            var success2 = _textSectionService.TryUpdate(1, null, "Second change", out var errmsg2);
            Assert.IsTrue(success2, errmsg2);
            _ctx.ChangeTracker.Clear();

            var diffs = _diffContentRepo.GetDiffs(DiffContentType.TextSection, 1).ToList();
            Assert.AreEqual(2, diffs.Count);
            // 按 Created 降序排列（GetDiffs 返回 IQueryable，需排序后验证）
            var ordered = diffs.OrderByDescending(d => d.Created).ToList();
            Assert.AreEqual(5, ordered[0].RemovedChars); // "First"
            Assert.AreEqual(6, ordered[0].AddedChars);   // "Second"
            Assert.AreEqual(16, ordered[1].RemovedChars); // "Modified content"
            Assert.AreEqual(12, ordered[1].AddedChars);   // "First change"
        }
    }
}
