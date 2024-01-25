using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.Identities
{
    public class User : IDbModel
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string? Name { get; set; }
        [MaxLength(32)]
        public string? PwdEncrypted { get; set; }
        public int AvatarMaterialId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
