using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Test.TestSupport;
using Newtonsoft.Json;

namespace FCloud3.Repos.Test.Identities
{
    [TestClass]
    public class UserToGroupRepoTest
    {
        private readonly FCloudContext _context;
        private readonly UserToGroupRepo _repo;
        public UserToGroupRepoTest()
        {
            _context = FCloudMemoryContext.Create();
            _repo = new UserToGroupRepo(_context, new(_context), new StubUserIdProvider(1));
            _repo.ClearCache();
            var time = new DateTime(2024, 1, 6);
            _context.Users.AddRange(new List<User>()
            {
                new() { Name = "李老师", Type = UserType.SuperAdmin }, //1
                new() { Name = "小红", Type = UserType.Member },       //2
                new() { Name = "小明", Type = UserType.Member },       //3
                new() { Name = "小赵", Type = UserType.Member },       //4
            });
            _context.UserGroups.AddRange(new List<UserGroup>()
            {
                new() { Name = "机器学习科研项目组", OwnerUserId = 1},   //1
                new() { Name = "LOL开黑小组" , OwnerUserId = 2}        //2
            });
            _context.UserToGroups.AddRange(new List<UserToGroup>()
            {
                new(){ UserId = 1, GroupId = 1, Type = UserToGroupType.Member, Updated = time},
                new(){ UserId = 2, GroupId = 1, Type = UserToGroupType.Member, Updated = time},
                new(){ UserId = 3, GroupId = 1, Type = UserToGroupType.Member, Updated = time},
                new(){ UserId = 3, GroupId = 2, Type = UserToGroupType.Member, Updated = time},
                new(){ UserId = 4, GroupId = 2, Type = UserToGroupType.Member, Updated = time},
                new(){ UserId = 1, GroupId = 2, Type = UserToGroupType.Inviting, Updated = time}
            });
            _context.SaveChanges();
        }
        
        [TestMethod]
        [DataRow(1, "1,2,3")]
        [DataRow(2, "3,4")]
        public void GetMembers(int groupId, string expectedMemberIdsStr)
        {
            List<int> expectedMemberIds = TestStrParse.IntList(expectedMemberIdsStr);
            var actualMemberIds = _repo.GetMembers(groupId);
            CollectionAssert.AreEqual(expectedMemberIds, actualMemberIds);
        }
        
        [TestMethod]
        [DataRow("1","1:1,2,3")]
        [DataRow("2","2:3,4")]
        [DataRow("1,2", "1:1,2,3  2:3,4")]
        public void GetDict(string groupIdsStr, string expectedDictStr)
        {
            List<int> groupIds = TestStrParse.IntList(groupIdsStr);
            Dictionary<int, List<int>> expectedDict = TestStrParse.IntDictInt(expectedDictStr);
            var actualDict = _repo.GetMembersDict(groupIds);
            Assert.AreEqual(
                JsonConvert.SerializeObject(expectedDict),
                JsonConvert.SerializeObject(actualDict));
        }

        [TestMethod]
        [DataRow(false, 1, 4, DisplayName = "邀请中的关系不算同组")]
        [DataRow(true, 1, 2, DisplayName = "是同组1")]
        [DataRow(true, 3, 4, DisplayName = "是同组2")]
        [DataRow(false, 2, 4 ,DisplayName = "不是同组")]
        public void IsInSameGroup(bool expected, int userA, int userB)
        {
            Assert.AreEqual(expected, _repo.IsInSameGroup(userA, userB));
        }

        [TestMethod]
        [DataRow(1, 2, true, false, UserToGroupType.Inviting, DisplayName = "已经邀请过应该失败")]
        [DataRow(2, 2, true, true, UserToGroupType.Inviting, DisplayName = "未邀请过应该成功，关系设为邀请中")]
        [DataRow(3, 2, true, false, UserToGroupType.Member, DisplayName = "已经是成员应该失败")]
        [DataRow(1, 2, false, false, UserToGroupType.Inviting, DisplayName = "超管：已经邀请过应该失败")]
        [DataRow(2, 2, false, true, UserToGroupType.Member, DisplayName = "超管：未邀请过应该成功，直接成为成员")]
        [DataRow(3, 2, false, false, UserToGroupType.Member, DisplayName = "超管：已经是成员应该失败")]
        public void AddUserToGroup(int userId, int groupId, bool needAudit, bool shouldSuccess, UserToGroupType? expectedRelation)
        {
            bool actualSuccess = _repo.AddUserToGroup(userId, groupId, needAudit, out _);
            Assert.AreEqual(shouldSuccess, actualSuccess);
            var actualRelation = _repo.GetRelation(groupId, userId)?.Type;
            Assert.AreEqual(expectedRelation, actualRelation);
            var relationCount = _repo.All.Where(x => x.GroupId == groupId && x.UserId == userId).Count();
            Assert.AreEqual(1, relationCount);
        }

        [TestMethod]
        [DataRow(2, 1, true, true, UserToGroupType.Member, DisplayName = "同意，成为成员")]
        [DataRow(2, 1, false, true, null, DisplayName = "拒绝，删除关系")]
        [DataRow(2, 2, true, false, null, DisplayName = "未邀请过，操作失败")]
        [DataRow(2, 3, true, false, UserToGroupType.Member, DisplayName = "本就是成员，操作失败")]
        public void InvitationAnswer(int groupId, int userId, bool accept, bool expectedSuccess, UserToGroupType? expectedRelation)
        {
            bool actualSuccess;
            if (accept)
                actualSuccess = _repo.AcceptInvitaion(userId, groupId, out _);
            else
                actualSuccess = _repo.RejectInvitaion(userId, groupId, out _);
            Assert.AreEqual(expectedSuccess, actualSuccess);
            var actualRelation = _repo.GetRelation(groupId, userId)?.Type;
            Assert.AreEqual(expectedRelation, actualRelation);
            
            var relationCount = _repo.Existing.Where(x => x.GroupId == groupId && x.UserId == userId).Count();
            var expectedCount = expectedRelation is not null ? 1 : 0;
            Assert.AreEqual(expectedCount, relationCount);
        }

        [TestMethod]
        [DataRow(3, 1, true, 2)]
        [DataRow(4, 1, false, 3)]
        [DataRow(1, 2, true, 2)]
        public void RemoveUserFromGroup(int userId, int groupId, bool expectedSuccess, int expectedMemberCount)
        {
            var actualSuccess = _repo.RemoveUserFromGroup(userId, groupId, out _);
            Assert.AreEqual(expectedSuccess, actualSuccess);
            var members = _repo.GetMembers(groupId);
            var actualMemberCount = members.Count;
            Assert.AreEqual(expectedMemberCount, actualMemberCount);
        }
    }
}