using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Sys;
using FCloud3.Repos.Test.TestSupport;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace FCloud3.Repos.Test.Identities
{
    [TestClass]
    public class AuthGrantRepoTest
    {
        private readonly FCloudContext _context;
        private readonly AuthGrantRepo _repo;
        public AuthGrantRepoTest()
        {
            _context = FCloudMemoryContext.Create() as FCloudContext;

            _context.Users.AddRange(new List<User>()
            {
                new() { Name = "user1", Type = UserType.SuperAdmin },
                new() { Name = "user2", Type = UserType.Member }
            });
            
            var initials = new List<AuthGrant>()
            {
                new() { CreatorUserId = 1, On = AuthGrantOn.WikiItem, OnId = 10, Order = 1},                    //1
                new() { CreatorUserId = 1, On = AuthGrantOn.WikiItem, OnId = 10, Order = 0},                    //2
                new() { CreatorUserId = 1, On = AuthGrantOn.WikiItem, OnId = AuthGrant.onIdForAll, Order = 1},  //3
                new() { CreatorUserId = 1, On = AuthGrantOn.WikiItem, OnId = AuthGrant.onIdForAll, Order = 0},  //4
                new() { CreatorUserId = 1, On = AuthGrantOn.WikiItem, OnId = 11, Order = 0},                    //5
                new() { CreatorUserId = 2, On = AuthGrantOn.WikiItem, OnId = 11, Order = 1},                    //6
                new() { CreatorUserId = 1, On = AuthGrantOn.WikiItem, OnId = 11, Order = 2},                    //7
                new() { CreatorUserId = 2, On = AuthGrantOn.WikiItem, OnId = AuthGrant.onIdForAll, Order = 1},  //8
                new() { CreatorUserId = 2, On = AuthGrantOn.WikiItem, OnId = AuthGrant.onIdForAll, Order = 0},  //9
                new() { CreatorUserId = 1, On = AuthGrantOn.Dir, OnId = 10, Order = 0},                         //10
                new() { CreatorUserId = 1, On = AuthGrantOn.Dir, OnId = 10, Order = 1},                         //11
                new() { CreatorUserId = 1, On = AuthGrantOn.Dir, OnId = AuthGrant.onIdForAll, Order = 0},       //12
                new() { CreatorUserId = 1, On = AuthGrantOn.Dir, OnId = AuthGrant.onIdForAll, Order = 1},       //13
                //新加的 id为14
            };
            initials.ForEach(i => i.Updated = new DateTime(2024, 1, 6));
            _context.AddRange(initials);
            _context.SaveChanges();

            _repo = new AuthGrantRepo(_context, new StubUserIdProvider(1));
            _repo.ClearCache();
        }

        /// <summary>
        /// 获取某对象的所有授权<br/>
        /// 如果是某个特定对象(onId != onIdForAll)，应该获取该对象所有者的全局设置(排前面)，加上该对象的本地设置(排后面)
        /// 如果是“所有我的”对象(onId == onIdForAll)，应该获取当前登录用户的该类型全局设置
        /// </summary>
        public static IEnumerable<object[]> GetByOnTestData()
        {
            yield return new object[] { AuthGrantOn.WikiItem, 10, 1, "4,3,2,1" };
            yield return new object[] { AuthGrantOn.WikiItem, 11, 1, "4,3,5,6,7" };
            yield return new object[] { AuthGrantOn.WikiItem, AuthGrant.onIdForAll, 0, "4,3" };
            yield return new object[] { AuthGrantOn.Dir, 10, 1, "12,13,10,11" };
            yield return new object[] { AuthGrantOn.Dir, AuthGrant.onIdForAll, 1, "12,13" };
        }

        [TestMethod]
        [DynamicData(nameof(GetByOnTestData))]
        public void GetByOn(AuthGrantOn on, int onId, int owner, string expectedIdsStr)
        {
            var expectedIds = expectedIdsStr.Split(',').ToList().ConvertAll(int.Parse);
            var actualIds = _repo.GetByOn(on, onId, owner).ConvertAll(x=>x.Id);
            CollectionAssert.AreEqual(expectedIds, actualIds);
        }

        public static IEnumerable<object[]> InsertNewTestData()
        {
            yield return new object[] { AuthGrantOn.WikiItem, 10, 1, "4,3,2,1,14", 2 };
            yield return new object[] { AuthGrantOn.WikiItem, AuthGrant.onIdForAll, 1, "4,3,14", 2 };
            yield return new object[] { AuthGrantOn.Dir, 10, 1, "12,13,10,11,14", 2 };
            yield return new object[] { AuthGrantOn.Dir, AuthGrant.onIdForAll, 1, "12,13,14", 2 };
        }

        [TestMethod]
        [DynamicData(nameof(InsertNewTestData))]
        public void InsertNew(AuthGrantOn on, int onId, int owner, string expectedIdsStr, int expectedInsertedOrder)
        {
            var expectedIds = expectedIdsStr.Split(',').ToList().ConvertAll(int.Parse);
            var inserted = new AuthGrant() { On = on, OnId = onId };
            _repo.TryAdd(inserted , out _);
            
            Assert.AreEqual(expectedInsertedOrder, inserted.Order);
            var actualIds = _repo.GetByOn(on, onId, owner).ConvertAll(x=>x.Id);
            CollectionAssert.AreEqual(expectedIds, actualIds);
            
            _repo.ChangeTracker.Clear();
            inserted = _repo.GetById(14)!;
            Assert.AreEqual(expectedInsertedOrder, inserted.Order);
        }

        public static IEnumerable<object[]> RemoveTestData()
        {
            yield return new object[] { 2, 1, "4,3,1", "0,1,0" };
            yield return new object[] { 1, 1, "4,3,2", "0,1,0" };
            yield return new object[] { 4, 1, "3", "0" };
            yield return new object[] { 3, 1, "4", "0" };
            yield return new object[] { 5, 1, "4,3,6,7", "0,1,0,1" };
            yield return new object[] { 6, 1, "4,3,5,7", "0,1,0,1" };
            yield return new object[] { 7, 1, "4,3,5,6", "0,1,0,1" };
        }

        [TestMethod]
        [DynamicData(nameof(RemoveTestData))]
        public void Remove(int deleteTargetId, int owner, string expectedIdsStr, string expectedOrdersStr)
        {
            var expectedIds = expectedIdsStr.Split(',').ToList().ConvertAll(int.Parse);
            var expectedOrders = expectedOrdersStr.Split(',').ToList().ConvertAll(int.Parse);
            var target = _repo.GetById(deleteTargetId)!;
            var on = target.On;
            var onId = target.OnId;
            _repo.TryRemove(target, out _);
            var actuals = _repo.GetByOn(on, onId, owner);
            var actualIds = actuals.ConvertAll(x => x.Id);
            var actualOrders = actuals.ConvertAll(x => x.Order);
            CollectionAssert.AreEqual(expectedIds, actualIds);
            CollectionAssert.AreEqual(expectedOrders, actualOrders);
        }
    }
}