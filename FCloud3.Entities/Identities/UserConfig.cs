namespace FCloud3.Entities.Identities
{
    public class UserConfig : IDbModel
    {
        public int Id { get; set; }
        public UserConfigType Type { get; set; }
        public string? ValueStr0 { get; set; }
        public int ValueNum0 { get; set; }
        public int ValueNum1 { get; set; }
        public int ValueNum2 { get; set; }
        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }

    public enum UserConfigType
    {
        WikiStyle = 11,
        WikiCCLicense = 12
    }
}