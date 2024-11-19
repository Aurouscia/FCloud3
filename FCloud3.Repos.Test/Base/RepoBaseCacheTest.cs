using FCloud3.Repos.Sys;
using FCloud3.Repos.Test.Base.FakeImplementation;
using FCloud3.Repos.Test.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Test.Base
{
    [TestClass]
    public class RepoBaseCacheTest
    {
        private readonly SomeClassRepo _repo;
        private readonly SomeClassRepo _repoAnother;
        public RepoBaseCacheTest()
        {
            var ctx = FCloudContextWithSomeClass.Create();
            var userIdProvider = new StubUserIdProvider(2);
            _repo = new SomeClassRepo(ctx, userIdProvider);
            _repoAnother = new SomeClassRepo(ctx, userIdProvider);

            var time = new DateTime(2024, 1, 6);
            List<SomeClass> items = [
                new(){ Name = "Au", Number1 = 32, Number2 = 11, Number3 = 12, Updated = time },
                new(){ Name = "Bu", Number1 = 64, Number2 = 21, Number3 = 22 },
                new(){ Name = "Cu", Number1 = 128, Number2 = 31, Number3 = 32 },
                ];
            //即使有的行的Update为默认值，也不影响缓存同步
            ctx.AddRange(items);
            ctx.SaveChanges();
            _repo.ClearCache();
        }

        [TestMethod]
        public void Read()
        {
            var items1 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items1, [32, 64, 128]);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);

            //同一个Scope内，如果没有进行过更改（没有调用过RepoBase.DataChanged）
            //那么再次查询不会去查数据库
            var item1 = _repo.CachedItemById(1);
            AssertObjectStatus(item1!, 32);
            var item2 = _repo.CachedItemById(2);
            AssertObjectStatus(item2!, 64);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);

            var items2 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items2, [32, 64, 128]);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);

            var item3 = _repo.CachedItemById(1);
            AssertObjectStatus(item3!, 32);
            var item4 = _repo.CachedItemById(2);
            AssertObjectStatus(item4!, 64);
            var item5 = _repo.CachedItemById(4);
            Assert.IsNull(item5);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);
        }

        [TestMethod]
        public void Append()
        {
            var items1 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items1, [32, 64, 128]);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);

            _repo.Add(new() { Number1 = 256 });

            var item1 = _repo.CachedItemById(4);
            AssertObjectStatus(item1!, 256);
            Assert.AreEqual(2, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(4, _repo.RepoCacheDictSyncFetchedRows);

            var items2 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items2, [32, 64, 128, 256]);
            Assert.AreEqual(2, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(4, _repo.RepoCacheDictSyncFetchedRows);

            _repo.Add(new() { Number1 = 512 });

            var items3 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items3, [32, 64, 128, 256, 512]);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(5, _repo.RepoCacheDictSyncFetchedRows);

            var item2 = _repo.CachedItemById(5);
            AssertObjectStatus(item2!, 512);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(5, _repo.RepoCacheDictSyncFetchedRows);
        }

        [TestMethod]
        public void Mutate()
        {
            var items1 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items1, [32, 64, 128]);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);

            var first = _repo.GetByIdEnsure(1);
            first.Number1 = 16;
            _repo.Update(first);

            var item1 = _repo.CachedItemById(1);
            AssertObjectStatus(item1!, 16);
            Assert.AreEqual(2, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(4, _repo.RepoCacheDictSyncFetchedRows);

            var items2 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items2, [16, 64, 128]);
            Assert.AreEqual(2, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(4, _repo.RepoCacheDictSyncFetchedRows);

            var second = _repo.GetByIdEnsure(2);
            second.Number1 = 32;
            _repo.Update(second);

            var items3 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items3, [16, 32, 128]);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(5, _repo.RepoCacheDictSyncFetchedRows);

            var item2 = _repo.CachedItemById(2);
            AssertObjectStatus(item2!, 32);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(5, _repo.RepoCacheDictSyncFetchedRows);
        }

        [TestMethod]
        public void Delete()
        {
            var items1 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items1, [32, 64, 128]);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);
            Assert.AreEqual(3, _repo.CachedItemsCount());

            _repo.Remove(1);

            var items2 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items2, [64, 128]);
            Assert.AreEqual(2, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);
            Assert.AreEqual(2, _repo.CachedItemsCount());

            var item1 = _repo.CachedItemById(1);
            Assert.IsNull(item1);
            Assert.AreEqual(2, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);
            Assert.AreEqual(2, _repo.CachedItemsCount());
        }

        [TestMethod]
        public void AvoidUnnecessarySync()
        {
            var items1 = _repo.AllCachedItems().ToList();
            AssertObjectsStatus(items1, [32, 64, 128]);
            Assert.AreEqual(1, _repo.RepoCacheDictSyncTimes);
            Assert.AreEqual(3, _repo.RepoCacheDictSyncFetchedRows);

            //另一个Scope的repo进来，先去Lu表查最新时间，决定不需要Sync
            var items2 = _repoAnother.AllCachedItems().ToList();
            AssertObjectsStatus(items2, [32, 64, 128]);
            Assert.AreEqual(0, _repoAnother.RepoCacheDictSyncTimes);
            Assert.AreEqual(0, _repoAnother.RepoCacheDictSyncFetchedRows);

            var items3 = _repoAnother.AllCachedItems().ToList();
            AssertObjectsStatus(items3, [32, 64, 128]);
            Assert.AreEqual(0, _repoAnother.RepoCacheDictSyncTimes);
            Assert.AreEqual(0, _repoAnother.RepoCacheDictSyncFetchedRows);
        }

        private static void AssertObjectsStatus(List<SomeClassCacheModel> items, List<int> num1s)
        {
            Assert.AreEqual(num1s.Count, items.Count);
            for(int i = 0; i < num1s.Count; i++)
            {
                AssertObjectStatus(items[i], num1s[i]);
            }
        }
        private static void AssertObjectStatus(SomeClassCacheModel item, int num1)
        {
            Assert.AreEqual(num1, item.Number1);
        }
    }
}
