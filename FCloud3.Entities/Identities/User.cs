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
        public UserType Type { get; set; }
        [MaxLength(64)]
        public string? Desc { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }

    public enum UserType
    {
        Tourist = 0,
        Member = 1,
        Admin = 8,
        SuperAdmin = 9
    }
}
