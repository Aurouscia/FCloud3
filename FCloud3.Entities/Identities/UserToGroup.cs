using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Identities
{
    public class UserToGroup:IDbModel, IRelation
    {
        public int Id { get; set; }
        public bool ShowLabel { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public int Order { get; set; }
        public UserToGroupType Type { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public int RelationMainId => GroupId;
        public int RelationSubId => UserId;
    }
    public enum UserToGroupType
    {
        Inviting = 0,
        Member = 1
    }
    public static class UserToGroupTypeExtension
    {
        public static bool IsFormalMember(this UserToGroupType type)
        {
            if(type == UserToGroupType.Member)
                return true;
            return false;
        }
        public static bool IsInviting(this UserToGroupType type)
        {
            if (type == UserToGroupType.Inviting)
                return true;
            return false;
        }
        public static IQueryable<UserToGroup> FilterFormalMember(this IQueryable<UserToGroup> q)
            => q.Where(x => x.Type == UserToGroupType.Member);
    }
}
