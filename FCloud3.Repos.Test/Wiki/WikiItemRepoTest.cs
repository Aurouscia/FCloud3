using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Sys;
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
        public WikiItemRepoTest()
        {
            var ctx = FCloudMemoryContext.Create();
            _repo = new(ctx, new StubUserIdProvider(1));
            var list = new List<WikiItem>()
            {
                new() { Id = 1, Created = _initalTime, Updated = _initalTime },
                new() { Id = 2, Created = _initalTime, Updated = _initalTime },
                new() { Id = 3, Created = _initalTime, Updated = _initalTime },
                new() { Id = 4, Created = _initalTime, Updated = _initalTime }
            };
            ctx.AddRange(list);
            ctx.SaveChanges();
        }
        private void CheckUpdated(List<int> expectUpdated)
        {
            _repo.ChangeTracker.Clear();
            var updated = _repo.Existing.ToList()
                .FindAll(x => x.Updated > _shouldBeBiggerTime)
                .ConvertAll(x => x.Id);
            CollectionAssert.AreEquivalent(expectUpdated, updated);
        }
        
        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void UpdateTimeSingle(int id)
        {
            _repo.UpdateTimeAndLu(id);
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
            _repo.UpdateTimeAndLu(ids);
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
            _repo.UpdateTimeAndLu(q);
            CheckUpdated(ids);
        }
    }
}