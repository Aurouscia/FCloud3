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
        public AuthGrantServiceTest()
        {
            var provider = new TestingServiceProvider(1);

            _svc = provider.Get<AuthGrantService>();
            _svc.DisableBuiltIn();
            _userIdProvider = provider.Get<StubUserIdProvider>();
            _ctx = provider.Get<FCloudContext>();
            _userGroupService = provider.Get<UserGroupService>();
            _authGrantRepo = provider.Get<AuthGrantRepo>();

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
                new() { OwnerUserId = 1, Updated = time },
                new() { OwnerUserId = 2, Updated = time },
                new() { OwnerUserId = 3, Updated = time },
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

        [TestMethod]
        [DataRow(1, 1, true)]
        [DataRow(1, 2, false)]
        [DataRow(2, 1, false)]
        [DataRow(2, 2, true)]
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

        [TestMethod]
        [DataRow(2, 2, true, true)]
        [DataRow(3, 2, false, true)]
        [DataRow(3, 3, true, true)]
        [DataRow(2, 3, false, true)]
        [DataRow(2, 2, true, false)]
        [DataRow(3, 2, false, false)]
        [DataRow(3, 3, true, false)]
        [DataRow(2, 3, false, false)]
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
        
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
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
        
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
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

        [TestMethod]
        [DataRow("u2", "", 2, true)]
        [DataRow("e!  u2", "", 2, true)]
        [DataRow("e  u2!", "", 2, false)]
        [DataRow("u2  e!", "", 2, false)]
        [DataRow("u2!  e", "", 2, true)]
        [DataRow("g2", "", 3, true)]
        [DataRow("g2!  u3", "", 3, true)]
        [DataRow("g2  u3!", "", 3, false)]
        [DataRow("u3!  g2", "", 3, true)]
        [DataRow("u3  g2!", "", 3, false)]
        [DataRow("g2!  e  u3!", "", 3, false)]
        [DataRow("g2  e!  u3", "", 3, true)]
        [DataRow("e!  g2  u3!", "", 3, false)]
        [DataRow("e!  g2  u3", "", 3, true)]
        [DataRow("e  g2!  u3", "", 3, true)]
        [DataRow("e  g2!  u3!", "", 3, false)]
        [DataRow("", "e", 3, true)]
        [DataRow("u3", "e", 3, true)]
        [DataRow("u3!", "e", 3, false)]
        [DataRow("", "g2", 3, true)]
        [DataRow("u3", "g2", 3, true)]
        [DataRow("u3!", "g2", 3, false)]
        [DataRow("u3", "u3!", 3, true)]
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
    }
}