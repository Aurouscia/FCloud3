using System.ComponentModel;

namespace FCloud3.Entities.Wiki
{
    public class WikiItem : IDbModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? UrlPathName { get; set; }
        public int OwnerUserId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
