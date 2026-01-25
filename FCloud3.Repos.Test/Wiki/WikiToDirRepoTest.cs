using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Test.TestSupport;
using FCloud3.Repos.Wiki;

namespace FCloud3.Repos.Test.Wiki
{
    [TestClass]
    public class WikiToDirRepoTest
    {
        private readonly WikiToDirRepo _repo;
        public WikiToDirRepoTest()
        {
            var ctx = FCloudMemoryContext.Create();
            _repo = new(ctx, new StubUserIdProvider(1));
            var list = new List<WikiToDir>()
            {
                new() { DirId = 1, WikiId = 1 },
                new() { DirId = 1, WikiId = 2 },
                new() { DirId = 1, WikiId = 3 },
                new() { DirId = 2, WikiId = 2 },
                new() { DirId = 2, WikiId = 3 },
                new() { DirId = 2, WikiId = 4 },
            };
            ctx.AddRange(list);
            ctx.SaveChanges();
        }

        public static IEnumerable<object[]> AddTestData()
        {
            yield return new object[] { 2, "4,5,6", "2,3,4,5,6" };
            yield return new object[] { 1, "4,5,6", "1,2,3,4,5,6" };
            yield return new object[] { 0, "4,5,6", "" };
        }

        [TestMethod]
        [DynamicData(nameof(AddTestData))]
        public void Add(int dirId, string addIdsStr, string expectedIdsStr)
        {
            var addIds = TestStrParse.IntList(addIdsStr);
            var expectedIds = TestStrParse.IntList(expectedIdsStr);
            if (dirId <= 0)
            {
                Assert.IsFalse(_repo.AddWikisToDir(addIds, dirId, out _));
                return;
            }
            Assert.IsTrue(_repo.AddWikisToDir(addIds, dirId, out _));
            var getWs = _repo.GetWikiIdsByDir(dirId);
            CollectionAssert.AreEquivalent(expectedIds, getWs);

            var getDirs1 = _repo.GetDirIdsByWikiIds([5, 6]).ToList();
            var getDirs2 = _repo.GetDirIdsByWikiId(5).ToList();
            CollectionAssert.AreEquivalent(new List<int> { dirId }, getDirs1);
            CollectionAssert.AreEquivalent(new List<int> { dirId }, getDirs2);
            var getDirs3 = _repo.GetDirIdsByWikiIds([2 ,3]).ToList();
            var getDirs4 = _repo.GetDirIdsByWikiId(2).ToList();
            CollectionAssert.AreEquivalent(new List<int> { 1, 2 }, getDirs3);
            CollectionAssert.AreEquivalent(new List<int> { 1, 2 }, getDirs4);
        }

        public static IEnumerable<object[]> RemoveTestData()
        {
            yield return new object[] { 1, "2,3,4", "1" };
            yield return new object[] { 1, "3,4", "1,2" };
        }

        [TestMethod]
        [DynamicData(nameof(RemoveTestData))]
        public void Remove(int dirId, string removeIdsStr, string expectedIdsStr)
        {
            var removeIds = TestStrParse.IntList(removeIdsStr);
            var expectedIds = TestStrParse.IntList(expectedIdsStr);
            Assert.IsTrue(_repo.RemoveWikisFromDir(removeIds, dirId, out _));
            var getWs = _repo.GetWikiIdsByDir(dirId);
            CollectionAssert.AreEquivalent(expectedIds, getWs);
        }
    }
}