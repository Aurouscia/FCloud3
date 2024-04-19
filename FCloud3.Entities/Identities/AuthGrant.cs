using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Identities
{
    public class AuthGrant : IDbModel, IRelation
    {
        public int Id { get; set; }
        public AuthGrantOn On { get; set; }
        public int OnId { get; set; }
        public AuthGrantTo To { get; set; }
        public int ToId { get; set; }
        public bool IsReject { get; set; }
        public int Order { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public int RelationMainId => OnId;
        public int RelationSubId => ToId;
        public const int maxCountOnSameOn = 25;
    }
    public enum AuthGrantOn
    {
        None = 0,
        WikiItem = 1,
        Dir = 2,
        Material = 3,
        FileItem = 4,
        TextSection = 10,
        FreeTable = 11,
        WikiPara = 19,
        User = 20,
        UserGroup = 21
    }
    public enum AuthGrantTo
    {
        None = 0,
        User = 1,
        UserGroup = 2,
        EveryOne = 3
    }
}
