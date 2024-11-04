using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.Sys
{
    public class LastUpdate
    {
        [Key]
        public LastUpdateType Type { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public enum LastUpdateType:byte
    {
        None = 0,
        WikiItem = 10,
        WikiTitleContain = 11,
        WikiItemRefedProps = 12,
        User = 20,
        AuthGrant = 21,
        UserToGroup = 22,
        UserGroup = 23,
        FileDir = 30,
        Material = 31
    }
}
