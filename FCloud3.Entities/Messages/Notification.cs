using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Messages
{
    public class Notification : IDbModel
    {
        public int Id { get; set; }
        public int Sender { get; set; }
        public int Receiver { get; set; }
        public NotifType Type { get; set; }
        public int Param1 { get; set; }
        public int Param2 { get; set; }
        public bool Read { get; set; }
        
        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }

    public enum NotifType
    {
        None = 0,
        CommentWiki = 10,
        CommentWikiReply = 11,
    }
}
