using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Entities.Wiki
{
    [Index(nameof(WikiId))]
    [Index(nameof(Str))]
    public class WikiRef
    {
        public int Id { get; set; }
        public int WikiId { get; set; }
        [MaxLength(32)]
        public string? Str { get; set; }
    }
}
