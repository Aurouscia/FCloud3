namespace FCloud3.Entities.DbModels.Identities
{
    public class User : IDbModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PwdMd5 { get; set; }
        public string? AvatarFileName { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
