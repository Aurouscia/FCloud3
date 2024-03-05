using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.WikiParsing
{
    public class WikiTemplate : IDbModel
    {
        public int Id { get; set; }
        [MaxLength(nameMaxLength)]
        public string? Name { get; set; }
        public string? Source { get; set; }
        public string? PreScripts { get; set; }
        public string? PostScripts { get; set; }
        public string? Styles { get; set; }
        public string? Demo { get; set; }
        public bool IsSingleUse { get; set; }
        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int nameMaxLength = 20;
    }
}
