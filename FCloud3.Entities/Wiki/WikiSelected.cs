using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Entities.Wiki
{
    [Index(nameof(WikiItemId))]
    public class WikiSelected:IDbModel
    {
        public int Id { get; set; }
        public int WikiItemId { get; set; }
        public int Order { get; set; }
        [MaxLength(introMaxLength)]
        public string? Intro { get; set; }
        public int DropAfterHr { get; set; }
        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
        
        public const int introMaxLength = 32;
    }
}