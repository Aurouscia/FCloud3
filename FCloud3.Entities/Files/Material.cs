using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Files
{
    public class Material : IDbModel
    {
        public int Id { get; set; }
        [MaxLength(displayNameMaxLength)]
        public string? Name { get; set; }
        [MaxLength(descMaxLength)]
        public string? Desc { get; set; }
        [MaxLength(storePathNameMaxLength)]
        public string? StorePathName { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int displayNameMaxLength = 16;
        public const int descMaxLength = 32;
        public const int storePathNameMaxLength = 50;
    }
}
