using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Sys;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Sys;

namespace FCloud3.Repos.Identities
{
    public class UserToGroupRepo(
        FCloudContext context,
        LastUpdateRepo lastUpdateRepo,
        ICommitingUserIdProvider userIdProvider
        ) : RepoBaseCache<UserToGroup, UserToGroupCacheModel>(
            context, lastUpdateRepo,userIdProvider)
    {

        public IQueryable<UserToGroup> ExistingAndShowLabel
            => Existing.Where(x => x.ShowLabel);

        /// <summary>
        /// 获取指定id组内所有成员（正在邀请的不算）
        /// </summary>
        /// <param name="groupId">用户组id</param>
        /// <returns>成员id列表</returns>
        public List<int> GetMembers(int groupId)
        {
            return AllCachedItemsFormalFiltered()
                .Where(x => x.GroupId == groupId)
                .Select(x => x.UserId).ToList();
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
            var allFormal = AllCachedItemsFormalFiltered();
            var q = (
                from g in allFormal
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
            base.Add(newRelation);
            errmsg = null;
            return true;
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
            base.Update(existing);
            errmsg = null;
            return true;
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
            base.Remove(existing);
            errmsg = null;
            return true;
        }
        public bool RemoveUserFromGroup(int userId, int groupId, out string? errmsg)
        {
            var existing = GetRelation(groupId, userId);
            if (existing is null)
            {
                errmsg = "未知错误：未找到群组关系";
                return false;
            }
            Remove(existing);
            errmsg = null; 
            return true;
        }
        
        public bool SetShowLabel(int userId, int groupId, bool show, out string? errmsg)
        {
            var r = GetRelation(groupId, userId);
            if (r is null)
            {
                errmsg = "不在该组中";
                return false;
            }
            r.ShowLabel = show;
            base.Update(r);
            errmsg = null;
            return true;
        }

        private IEnumerable<UserToGroupCacheModel> AllCachedItemsFormalFiltered()
            => AllCachedItems().Where(x => x.Type > UserToGroupType.Inviting);
        protected override IQueryable<UserToGroupCacheModel> ConvertToCacheModel(IQueryable<UserToGroup> q)
        {
            return q.Select(x => new UserToGroupCacheModel(
                x.Id, x.Updated, x.UserId, x.GroupId, x.Type));
        }

        protected override LastUpdateType GetLastUpdateType()
            => LastUpdateType.UserToGroup;
    }
    public class UserToGroupCacheModel(
        int id, DateTime updated, int userId, int groupId, UserToGroupType type)
        : CacheModelBase<UserToGroup>(id, updated)
    {
        public int UserId { get; } = userId;
        public int GroupId { get; } = groupId;
        public UserToGroupType Type { get; } = type;
    }
}
