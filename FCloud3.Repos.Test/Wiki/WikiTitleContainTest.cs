using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Test.TestSupport;
using FCloud3.Repos.Wiki;

namespace FCloud3.Repos.Test.Wiki
{
    [TestClass]
    public class WikiTitleContainTest
    {
        private readonly WikiTitleContainRepo _repo;
        public WikiTitleContainTest()
        {
            var ctx = FCloudMemoryContext.Create();
            _repo = new WikiTitleContainRepo(ctx, new StubUserIdProvider(1));
            List<WikiTitleContain> existing =
            [
                new() { WikiId = 1, Type = WikiTitleContainType.TextSection, ObjectId = 1 },  //1
                new() { WikiId = 2, Type = WikiTitleContainType.TextSection, ObjectId = 1 },  //2
                new() { WikiId = 3, Type = WikiTitleContainType.TextSection, ObjectId = 1, BlackListed = true },
                
                new() { WikiId = 2, Type = WikiTitleContainType.TextSection, ObjectId = 2 },  //4
                new() { WikiId = 3, Type = WikiTitleContainType.TextSection, ObjectId = 2, BlackListed = true },
                
                new() { WikiId = 2, Type = WikiTitleContainType.FreeTable, ObjectId = 1 },    //6
                new() { WikiId = 3, Type = WikiTitleContainType.FreeTable, ObjectId = 1, BlackListed = true },
            ];
            ctx.AddRange(existing);
            ctx.SaveChanges();
        }

        [TestMethod]
        public void Query()
        {
            var t1nb = _repo.GetByTypeAndObjId(WikiTitleContainType.TextSection, 1, true);
            var t1nb_ = _repo.NotBlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 1).ToList();
            CollectionAssert.AreEquivalent(new List<int>(){ 1, 2 }, t1nb.ConvertAll(x=>x.Id));
            CollectionAssert.AreEquivalent(new List<int>(){ 1, 2 }, t1nb_.ConvertAll(x=>x.Id));
            var t1all = _repo.GetByTypeAndObjId(WikiTitleContainType.TextSection, 1, false);
            CollectionAssert.AreEquivalent(new List<int>(){ 1, 2, 3 }, t1all.ConvertAll(x=>x.Id));
            var t1b = _repo.BlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 1).ToList();
            CollectionAssert.AreEquivalent(new List<int>(){ 3 }, t1b.ConvertAll(x=>x.Id));
        }

        [TestMethod]
        public void SetStatus()
        {
            List<WikiTitleContain> inToBlackList = _repo.GetRangeByIds([1]).ToList();
            List<WikiTitleContain> outOfBlackList = _repo.GetRangeByIds([3]).ToList();
            List<WikiTitleContain> newObjs =
            [
                new() { WikiId = 4, Type = WikiTitleContainType.TextSection, ObjectId = 1 },
                new() { WikiId = 5, Type = WikiTitleContainType.TextSection, ObjectId = 1 },
            ];
            _repo.SetStatus(inToBlackList, outOfBlackList, newObjs, out _);
            var t1nb = _repo.GetByTypeAndObjId(WikiTitleContainType.TextSection, 1, true);
            CollectionAssert.AreEquivalent(new List<int>(){ 2, 3, 4,5 }, t1nb.ConvertAll(x=>x.WikiId));
            var t1b = _repo.BlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 1).ToList();
            CollectionAssert.AreEquivalent(new List<int>(){ 1 }, t1b.ConvertAll(x=>x.WikiId));
        }

        [TestMethod]
        public void AutoRemoveDuplicate()
        {
            List<WikiTitleContain> inToBlackList = _repo.GetRangeByIds([1]).ToList();
            List<WikiTitleContain> outOfBlackList = _repo.GetRangeByIds([3]).ToList();
            List<WikiTitleContain> newObjs =
            [
                //意外重复插入了3
                new() { WikiId = 3, Type = WikiTitleContainType.TextSection, ObjectId = 1 },
                new() { WikiId = 4, Type = WikiTitleContainType.TextSection, ObjectId = 1 },
            ];
            _repo.SetStatus(inToBlackList, outOfBlackList, newObjs, out _);
            //此处不会修复
            var t1nb = _repo.NotBlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 1).ToList();
            Assert.AreEqual(4, t1nb.Count);
            //此处会修复
            t1nb = _repo.GetByTypeAndObjId(WikiTitleContainType.TextSection, 1, true);
            Assert.AreEqual(3, t1nb.Count);
            CollectionAssert.AreEquivalent(new List<int>(){ 2, 3, 4 }, t1nb.ConvertAll(x=>x.WikiId));
            var t1b = _repo.BlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 1).ToList();
            CollectionAssert.AreEquivalent(new List<int>(){ 1 }, t1b.ConvertAll(x=>x.WikiId));
        }
    }
}