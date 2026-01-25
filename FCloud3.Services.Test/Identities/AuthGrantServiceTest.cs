using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Sys;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using FCloud3.Services.Etc.Cache;
using FCloud3.Services.Identities;
using FCloud3.Services.Messages;
using FCloud3.Services.Test.TestSupport;
using FCloud3.Services.Wiki;

namespace FCloud3.Services.Test.Identities
{
    [TestClass]
    public class AuthGrantServiceTest
    {
        private readonly AuthGrantService _svc;
        private readonly StubUserIdProvider _userIdProvider;
        private readonly FCloudContext _ctx;
        private readonly UserGroupService _userGroupService;
        private readonly AuthGrantRepo _authGrantRepo;
        private readonly WikiItemService _wikiItemService;
        public AuthGrantServiceTest()
        {
            var provider = new TestingServiceProvider(1);

            _svc = provider.Get<AuthGrantService>();
            _svc.DisableBuiltIn();
            _userIdProvider = provider.Get<StubUserIdProvider>();
            _ctx = provider.Get<FCloudContext>();
            _userGroupService = provider.Get<UserGroupService>();
            _authGrantRepo = provider.Get<AuthGrantRepo>();
            _wikiItemService = provider.Get<WikiItemService>();

            _authGrantRepo.ClearCache();
            provider.Get<AuthResCacheHost>().Clear();
            provider.Get<UserGroupRepo>().ClearCache();
            provider.Get<UserToGroupRepo>().ClearCache();

            var time = new DateTime(2024, 1, 1);

            List<User> userList =
            [
                new() { Name = "u1" },
                new() { Name = "u2" },
                new() { Name = "u3" }
            ];
            List<UserGroup> userGroups =
            [
                new() { Name = "g1", OwnerUserId = 1, Updated = time },
                new() { Name = "g2", OwnerUserId = 2, Updated = time }
            ];
            List<UserToGroup> userToGroups =
            [
                new() { UserId = 1, GroupId = 1, Type = UserToGroupType.Member, Updated = time },
                new() { UserId = 2, GroupId = 2, Type = UserToGroupType.Member, Updated = time },
            ];
            List<WikiItem> wikiItems =
            [
                new() { OwnerUserId = 1, Updated = time, Title = "a", UrlPathName = "a" },
                new() { OwnerUserId = 2, Updated = time, Title = "b", UrlPathName = "b" },
                new() { OwnerUserId = 3, Updated = time, Title = "c", UrlPathName = "c" },
            ];
            _ctx.AddRange(userList); _ctx.AddRange(userGroups);
            _ctx.AddRange(userToGroups); _ctx.AddRange(wikiItems);
            _ctx.SaveChanges();
        }
        
        [TestMethod]
        public void SelfOwned()
        {
            _userIdProvider.UserId = 1;
            Assert.IsTrue(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            Assert.AreEqual(1, _svc.TestedCount);
            Assert.IsTrue(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            Assert.AreEqual(1, _svc.TestedCount);
            Assert.AreEqual(0, _authGrantRepo.RepoCacheDictSyncTimes);//所有者等于访问者的情况，会避免数据库查询
        }

        public static IEnumerable<object[]> OnlySelfCanEditTestData()
        {
            yield return new object[] { 1, 1, true };
            yield return new object[] { 1, 2, false };
            yield return new object[] { 2, 1, false };
            yield return new object[] { 2, 2, true };
        }

        [TestMethod]
        [DynamicData(nameof(OnlySelfCanEditTestData))]
        public void OnlySelfCanEdit(int loginUserId, int wikiId, bool expectSuccess)
        {
            _userIdProvider.UserId = loginUserId;
            AuthGrant grant = new() { On = AuthGrantOn.WikiItem, OnId = wikiId,To = AuthGrantTo.User, ToId = 3 };
            _ctx.Add(grant);
            _ctx.SaveChanges();
            Assert.AreEqual(expectSuccess, _svc.Add(new(){On = AuthGrantOn.WikiItem, OnId = wikiId }, out _));
            Assert.AreEqual(expectSuccess, _svc.SetOrder(AuthGrantOn.WikiItem, wikiId, [2, 1], out _));
            Assert.AreEqual(expectSuccess, _svc.Remove(1, out _));
        }

        public static IEnumerable<object[]> SingleUserGrantTestData()
        {
            yield return new object[] { 2, 2, true, true };
            yield return new object[] { 3, 2, false, true };
            yield return new object[] { 3, 3, true, true };
            yield return new object[] { 2, 3, false, true };
            yield return new object[] { 2, 2, true, false };
            yield return new object[] { 3, 2, false, false };
            yield return new object[] { 3, 3, true, false };
            yield return new object[] { 2, 3, false, false };
        }

        [TestMethod]
        [DynamicData(nameof(SingleUserGrantTestData))]
        public void SingleUserGrant(int toUser, int loginUser, bool expected, bool notReject)
        {
            _userIdProvider.UserId = loginUser;
            Assert.IsFalse(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            _userIdProvider.UserId = 1;
            _svc.Add(new() { On = AuthGrantOn.WikiItem, OnId = 1, To = AuthGrantTo.User, ToId = toUser, IsReject = !notReject}, out _);

            _ctx.ChangeTracker.Clear();//模拟Scope结束

            _userIdProvider.UserId = loginUser;
            Assert.AreEqual(expected && notReject, _svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            Assert.AreEqual(2, _authGrantRepo.RepoCacheDictSyncTimes);//中间新增了一次，导致需要重新查询
        }
        
        public static IEnumerable<object[]> EveryOneGrantTestData()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        [TestMethod]
        [DynamicData(nameof(EveryOneGrantTestData))]
        public void EveryOneGrant(bool isReject)
        {
            _userIdProvider.UserId = 2;
            Assert.IsFalse(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            _userIdProvider.UserId = 1;
            _svc.Add(new() { On = AuthGrantOn.WikiItem, OnId = 1, To = AuthGrantTo.EveryOne, IsReject = isReject}, out _);

            _ctx.ChangeTracker.Clear();//模拟Scope结束

            _userIdProvider.UserId = 2;
            Assert.AreEqual(!isReject, _svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            Assert.AreEqual(2, _authGrantRepo.RepoCacheDictSyncTimes);//中间新增了一次，导致需要重新查询
        }
        
        public static IEnumerable<object[]> UserGroupGrantTestData()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        [TestMethod]
        [DynamicData(nameof(UserGroupGrantTestData))]
        public void UserGroupGrant(bool isReject)
        {
            _userIdProvider.UserId = 3;
            //3号用户默认情况下无法访问1号词条
            Assert.IsFalse(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            _userIdProvider.UserId = 1;
            //1号用户设置2号群组访问1号词条权限
            _svc.Add(new() { On = AuthGrantOn.WikiItem, OnId = 1, To = AuthGrantTo.UserGroup, ToId = 2, IsReject = isReject}, out _);
            _ctx.ChangeTracker.Clear();//模拟Scope结束

            _userIdProvider.UserId = 3;
            //3号用户依然无法访问1号词条
            Assert.IsFalse(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            //3号用户加入2号群组
            _userGroupService.AddUserToGroup(3, 2, false, out _);
            _ctx.ChangeTracker.Clear();//模拟Scope结束

            //3号用户作为2号群组成员有权/无权访问1号词条
            Assert.AreEqual(!isReject, _svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            //3号用户离开2号群组
            _userGroupService.RemoveUserFromGroup(3, 2, out _);
            _ctx.ChangeTracker.Clear();//模拟Scope结束

            //3号用户再无法访问1号词条
            Assert.IsFalse(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            _userIdProvider.UserId = 2;
            //但2号群组中的2号用户依然可以
            Assert.AreEqual(!isReject, _svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            //解散2号群组
            _userGroupService.Dissolve(2, out _);
            _ctx.ChangeTracker.Clear();//模拟Scope结束

            //2号用户再不能访问词条
            Assert.IsFalse(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            //2号用户再不能访问词条（不会增加_svc.TestedCount）
            Assert.IsFalse(_svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            Assert.AreEqual(6, _svc.TestedCount);
            Assert.AreEqual(2, _authGrantRepo.RepoCacheDictSyncTimes);
            //一二次中间新增了一次，导致需要查询两次，二三四次中间并未新增/删除，Test需要的AuthGrant列表使用缓存
        }

        public static IEnumerable<object[]> CascadingTestData()
        {
            yield return new object[] { "u2", "", 2, true };
            yield return new object[] { "e!  u2", "", 2, true };
            yield return new object[] { "e  u2!", "", 2, false };
            yield return new object[] { "u2  e!", "", 2, false };
            yield return new object[] { "u2!  e", "", 2, true };
            yield return new object[] { "g2", "", 3, true };
            yield return new object[] { "g2!  u3", "", 3, true };
            yield return new object[] { "g2  u3!", "", 3, false };
            yield return new object[] { "u3!  g2", "", 3, true };
            yield return new object[] { "u3  g2!", "", 3, false };
            yield return new object[] { "g2!  e  u3!", "", 3, false };
            yield return new object[] { "g2  e!  u3", "", 3, true };
            yield return new object[] { "e!  g2  u3!", "", 3, false };
            yield return new object[] { "e!  g2  u3", "", 3, true };
            yield return new object[] { "e  g2!  u3", "", 3, true };
            yield return new object[] { "e  g2!  u3!", "", 3, false };
            yield return new object[] { "", "e", 3, true };
            yield return new object[] { "u3", "e", 3, true };
            yield return new object[] { "u3!", "e", 3, false };
            yield return new object[] { "", "g2", 3, true };
            yield return new object[] { "u3", "g2", 3, true };
            yield return new object[] { "u3!", "g2", 3, false };
            yield return new object[] { "u3", "u3!", 3, true };
        }

        [TestMethod]
        [DynamicData(nameof(CascadingTestData))]
        public void Cascading(string auths, string globalAuths, int loginUid, bool expected)
        {
            _userGroupService.AddUserToGroup(3, 2, false, out _);
            
            _userIdProvider.UserId = 1;
            var authGrants = TestStrParse.AuthGrants(auths, AuthGrantOn.WikiItem, 1);
            authGrants.ForEach(a => _svc.Add(a, out _));
            var globalAuthGrants = TestStrParse.AuthGrants(globalAuths, AuthGrantOn.WikiItem, AuthGrant.onIdForAll);
            globalAuthGrants.ForEach(a=> _svc.Add(a, out _));
            
            _userIdProvider.UserId = loginUid;
            Assert.AreEqual(expected, _svc.CheckAccess(AuthGrantOn.WikiItem, 1));
            Assert.AreEqual(1, _authGrantRepo.RepoCacheDictSyncTimes);
        }

        public static IEnumerable<object[]> TransferredTestData()
        {
            yield return new object[] { 1, 1 };
            yield return new object[] { 1, 2 };
            yield return new object[] { 1, 3 };
            yield return new object[] { 2, 1 };
            yield return new object[] { 2, 2 };
            yield return new object[] { 2, 3 };
        }

        [TestMethod]
        [DynamicData(nameof(TransferredTestData))]
        public void Transferred(int loginUserId, int wikiId)
        {
            int originalOwner = wikiId;
            bool originalAccess = loginUserId == originalOwner;
            _userIdProvider.UserId = originalOwner;
            Assert.IsTrue(_svc.CheckAccess(AuthGrantOn.WikiItem, wikiId));
            _userIdProvider.UserId = loginUserId;
            Assert.AreEqual(originalAccess, _svc.CheckAccess(AuthGrantOn.WikiItem, wikiId));

            _wikiItemService.Transfer(wikiId, loginUserId, true, out _);
            _ctx.ChangeTracker.Clear(); //模拟scope结束

            _userIdProvider.UserId = originalOwner;
            Assert.AreEqual(originalAccess, _svc.CheckAccess(AuthGrantOn.WikiItem, wikiId));
            _userIdProvider.UserId = loginUserId;
            Assert.IsTrue(_svc.CheckAccess(AuthGrantOn.WikiItem, wikiId));
        }
    }
}