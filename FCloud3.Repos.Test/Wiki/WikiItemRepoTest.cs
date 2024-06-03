using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.Etc.Caching.Abstraction;
using FCloud3.Repos.Test.TestSupport;
using FCloud3.Repos.Wiki;

namespace FCloud3.Repos.Test.Wiki
{
    [TestClass]
    public class WikiItemRepoTest
    {
        private readonly WikiItemRepo _repo;
        private readonly DateTime _initalTime = new DateTime(1970, 1, 1);
        private readonly DateTime _shouldBeBiggerTime = new DateTime(2000, 1, 1);
        private readonly WikiItemCaching _cache;
        private readonly WikiItemCaching _anotherCache;
        public WikiItemRepoTest()
        {
            var ctx = FCloudMemoryContext.Create();
            _cache = new WikiItemCaching(ctx, new FakeLogger<CachingBase<WikiItemCachingModel, WikiItem>>());
            _anotherCache = new WikiItemCaching(ctx, new FakeLogger<CachingBase<WikiItemCachingModel, WikiItem>>());
            _repo = new(ctx,new StubUserIdProvider(1), _cache);
            var list = new List<WikiItem>()
            {
                new() { Id = 1, Created = _initalTime, Updated = _initalTime },
                new() { Id = 2, Created = _initalTime, Updated = _initalTime },
                new() { Id = 3, Created = _initalTime, Updated = _initalTime },
                new() { Id = 4, Created = _initalTime, Updated = _initalTime }
            };
            ctx.AddRange(list);
            ctx.SaveChanges();
            _cache.Clear();
        }
        private void CheckUpdated(List<int> expectUpdated)
        {
            _repo.ChangeTracker.Clear();
            var updated = _repo.Existing.ToList()
                .FindAll(x => x.Updated > _shouldBeBiggerTime)
                .ConvertAll(x => x.Id);
            CollectionAssert.AreEquivalent(expectUpdated, updated);

            var updatedCached = _anotherCache.GetRange(expectUpdated);
            //更新时间是“缓存直接操作”，缓存内找不到的话也不会去查数据库
            //此时再GetRange会引发查询
            Assert.AreEqual(1, _anotherCache.QueriedTimes);
            Assert.AreEqual(expectUpdated.Count, _anotherCache.QueriedRows);
            updatedCached.ForEach(x => Assert.IsTrue(x.Update>_shouldBeBiggerTime));
        }
        
        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void UpdateTimeSingle(int id)
        {
            _repo.UpdateTime(id);
            CheckUpdated([id]);
        }
        
        [TestMethod]
        [DataRow("1")]
        [DataRow("1,2")]
        [DataRow("1,2,3")]
        [DataRow("1,2,3,4")]
        public void UpdateTimeMutiple(string idsStr)
        {
            List<int> ids = TestStrParse.IntList(idsStr);
            _repo.UpdateTime(ids);
            CheckUpdated(ids);
        }
        
        [TestMethod]
        [DataRow("1")]
        [DataRow("1,2")]
        [DataRow("1,2,3")]
        [DataRow("1,2,3,4")]
        public void UpdateTimeMutipleAsQueryable(string idsStr)
        {
            List<int> ids = TestStrParse.IntList(idsStr);
            var q = ids.AsQueryable();
            var res = _repo.UpdateTime(q);
            Assert.AreEqual(ids.Count, res);
            CheckUpdated(ids);
        }
    }
}