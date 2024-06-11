using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.Wiki
{
    public class WikiItem : IDbModel
    {
        public int Id { get; set; }
        [MaxLength(titleMaxLength)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [MaxLength(urlPathNameMaxLength)]
        public string? UrlPathName { get; set; }
        public int OwnerUserId { get; set; }
        public bool Sealed { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int titleMaxLength = 32;
        public const int urlPathNameMaxLength = 32;
    }
}
