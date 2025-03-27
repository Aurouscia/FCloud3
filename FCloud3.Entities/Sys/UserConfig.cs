namespace FCloud3.Entities.Sys
{
    public class UserConfig : IDbModel
    {
        public int Id { get; set; }
        public UserConfigOn On { get; set; }
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
    public enum UserConfigOn:byte
    {
        User = 11
    }
    public enum UserConfigType:byte
    {
        WikiStyle = 11,
        WikiCCLicense = 12
    }
}