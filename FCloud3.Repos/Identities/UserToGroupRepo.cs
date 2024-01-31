using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
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

        public bool AddUserToGroup(int userId, int groupId, out string? errmsg)
        {
            var existing = Existing.Where(x => x.UserId == userId && x.GroupId == groupId).FirstOrDefault();
            //TODO：超级管理员无需邀请即可直接使其成为成员
            if (existing is not null)
            {
                if (existing.Type == UserToGroupType.Inviting)
                    errmsg = "已经邀请过该用户";
                else
                    errmsg = "该用户本就在组中";
                return false;
            }
            var newRelation = new UserToGroup()
            {
                UserId = userId,
                GroupId = groupId,
                Type = UserToGroupType.Inviting
            };
            return TryAdd(newRelation, out errmsg);
        }
        public bool AcceptInvitaion(int userId, int groupId, out string? errmsg)
        {
            //TODO：验证身份是否与userId一致
            var existing = Existing.Where(x => x.UserId == userId && x.GroupId == groupId).FirstOrDefault();
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
            //TODO：验证身份是否与userId一致
            var existing = Existing.Where(x => x.UserId == userId && x.GroupId == groupId).FirstOrDefault();
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
            //TODO：验证身份是否与userId一致
            var existing = Existing.Where(x => x.UserId == userId && x.GroupId == groupId).FirstOrDefault();
            if (existing is null)
            {
                errmsg = "未知错误：未找到群组关系";
                return false;
            }
            return TryRemovePermanent(existing, out errmsg);
        }
    }
}
