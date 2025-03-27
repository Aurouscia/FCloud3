using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.Messages
{
    public class Message:IDbModel
    {
        public int Id { get; set; }
        public int CreatorUserId { get; set; }
        public int ReceiverId { get; set; }
        [MaxLength(contentMaxLength)]
        public string? Content { get; set; }
        public MessageContentType ContentType { get; set; }
        public bool Read { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        private const int contentMaxLength = 512;
    }
    public enum MessageContentType:byte
    {
        Text = 0
    }
}
