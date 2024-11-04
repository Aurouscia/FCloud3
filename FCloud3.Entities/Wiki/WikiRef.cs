using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.Wiki
{
    public class WikiRef
    {
        public int Id { get; set; }
        public int WikiId { get; set; }
        [MaxLength(32)]
        public string? Str { get; set; }
    }
}
