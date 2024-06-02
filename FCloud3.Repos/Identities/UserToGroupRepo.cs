using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Identities
{
    public class UserToGroupRepo : RepoBase<UserToGroup>
    {
        public UserToGroupRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }
        /// <summary>
        /// 获取指定id组内所有成员（正在邀请的不算）
        /// </summary>
        /// <param name="groupId">用户组id</param>
        /// <returns>成员id列表</returns>
        public List<int> GetMembers(int groupId)
        {
            return Existing
                .Where(x=>x.GroupId == groupId)
                .FilterFormalMember().Select(x=>x.UserId).ToList();
        }
        /// <summary>
        /// 获取指定id组内所有成员（正在邀请的不算）
        /// </summary>
        /// <param name="groupIds">用户组id列表</param>
        /// <returns>成员id字典，key为组id，value为其成员id列表</returns>
        public Dictionary<int,List<int>> GetMembersDict(List<int> groupIds)
        {
            if (groupIds.Count == 0)
                return new();

            var q = (
                from g in Existing.FilterFormalMember()
                where groupIds.Contains(g.GroupId)
                group g.UserId by g.GroupId).ToList();

            return q.ToDictionary(x => x.Key, x => x.ToList());
        }
        public bool IsInSameGroup(int user1,int user2)
        {
            var q =
                from rA in Existing.FilterFormalMember()
                from rB in Existing.FilterFormalMember()
                where rA.UserId == user1 && rB.UserId == user2
                where rA.GroupId == rB.GroupId
                select rA.GroupId;
            return q.Any();
        }
        public UserToGroup? GetRelation(int groupId, int userId)
            => Existing.Where(x => x.GroupId == groupId && x.UserId == userId).FirstOrDefault();
        /// <summary>
        /// 将用户加入用户组
        /// </summary>
        /// <param name="userId">目标用户id</param>
        /// <param name="groupId">目标组id</param>
        /// <param name="needAudit">是否需要目标用户同意</param>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public bool AddUserToGroup(int userId, int groupId, bool needAudit, out string? errmsg)
        {
            var existing = GetRelation(groupId, userId);
            if (existing is not null)
            {
                if (existing.Type == UserToGroupType.Inviting)
                    errmsg = "已经邀请过该用户";
                else
                    errmsg = "该用户本就在组中";
                return false;
            }
            UserToGroupType t = needAudit ? UserToGroupType.Inviting : UserToGroupType.Member; 
            var newRelation = new UserToGroup()
            {
                UserId = userId,
                GroupId = groupId,
                Type = t
            };
            return TryAdd(newRelation, out errmsg);
        }
        public bool AcceptInvitaion(int userId, int groupId, out string? errmsg)
        {
            var existing = GetRelation(groupId, userId);
            if(existing is null)
            {
                errmsg = "未知错误：未找到该邀请";
                return false;
            }
            if (existing.Type != UserToGroupType.Inviting)
            {
                errmsg = "未知错误：邀请状态异常";
                return false;
            }
            existing.Type = UserToGroupType.Member;
            return TryEdit(existing, out errmsg);
        }
        public bool RejectInvitaion(int userId, int groupId, out string? errmsg)
        {
            var existing = GetRelation(groupId, userId);
            if (existing is null)
            {
                errmsg = "未知错误：未找到该邀请";
                return false;
            }
            if (existing.Type != UserToGroupType.Inviting)
            {
                errmsg = "未知错误：邀请状态异常";
                return false;
            }
            return TryRemovePermanent(existing, out errmsg);
        }
        public bool RemoveUserFromGroup(int userId, int groupId, out string? errmsg)
        {
            var existing = GetRelation(groupId, userId);
            if (existing is null)
            {
                errmsg = "未知错误：未找到群组关系";
                return false;
            }
            return TryRemovePermanent(existing, out errmsg);
        }
    }
}
