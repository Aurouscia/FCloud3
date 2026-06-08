using FCloud3.DbContexts;
using FCloud3.Entities.Diff;
using FCloud3.Entities.Table;
using FCloud3.Repos.Diff;
using FCloud3.Repos.Table;
using FCloud3.Services.Table;
using FCloud3.Services.Test.TestSupport;

namespace FCloud3.Services.Test.Diff
{
    [TestClass]
    public class FreeTableDiffTest
    {
        private readonly FCloudContext _ctx;
        private readonly FreeTableService _freeTableService;
        private readonly DiffContentRepo _diffContentRepo;

        private const string OriginalTableData = """{"name":"","cells":[["A","B"],["C","D"]],"merges":[]}""";
        private const string ModifiedTableData = """{"name":"","cells":[["X","Y"],["C","D"]],"merges":[]}""";

        public FreeTableDiffTest()
        {
            var provider = new TestingServiceProvider();
            _freeTableService = provider.Get<FreeTableService>();
            _diffContentRepo = provider.Get<DiffContentRepo>();
            _ctx = provider.Get<FCloudContext>();

            var ft = new FreeTable
            {
                Name = "TestTable",
                Data = OriginalTableData
            };
            _ctx.FreeTables.Add(ft);
            _ctx.SaveChanges();
            _ctx.ChangeTracker.Clear();
        }

        [TestMethod]
        public void EditContent_CreatesDiff()
        {
            var success = _freeTableService.TryEditContent(1, ModifiedTableData, out var errmsg);
            Assert.IsTrue(success, errmsg);
            _ctx.ChangeTracker.Clear();

            var diffs = _diffContentRepo.GetDiffs(DiffContentType.FreeTable, 1).ToList();
            Assert.AreEqual(1, diffs.Count);
            Assert.AreEqual(1, diffs[0].ObjectId);
            Assert.AreEqual(DiffContentType.FreeTable, diffs[0].DiffType);
            Assert.IsTrue(diffs[0].RemovedChars > 0);
            Assert.IsTrue(diffs[0].AddedChars > 0);

            var singles = _diffContentRepo.DiffSingles
                .Where(x => x.DiffContentId == diffs[0].Id)
                .OrderBy(x => x.Index)
                .ToList();
            Assert.AreEqual(2, singles.Count);
            // 第一个 diff single："A" 被替换（在 JSON 中的位置）
            Assert.AreEqual(22, singles[0].Index);
            Assert.AreEqual("A", singles[0].Ori);
            Assert.AreEqual(1, singles[0].New);
            // 第二个 diff single："B" 被替换
            Assert.AreEqual(26, singles[1].Index);
            Assert.AreEqual("B", singles[1].Ori);
            Assert.AreEqual(1, singles[1].New);
        }

        [TestMethod]
        public void EditContent_SameData_NoDiff()
        {
            var success = _freeTableService.TryEditContent(1, OriginalTableData, out var errmsg);
            Assert.IsTrue(success, errmsg);
            _ctx.ChangeTracker.Clear();

            var diffs = _diffContentRepo.GetDiffs(DiffContentType.FreeTable, 1).ToList();
            Assert.AreEqual(0, diffs.Count);
        }
    }
}
