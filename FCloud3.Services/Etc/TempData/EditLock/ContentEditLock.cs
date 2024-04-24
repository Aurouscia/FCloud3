using Microsoft.EntityFrameworkCore;

namespace FCloud3.Services.Etc.TempData.EditLock
{
    [PrimaryKey(nameof(ObjectType), nameof(ObjectId))]
    public class ContentEditLock
    {
        public HeartbeatObjType ObjectType { get; set; }
        public int ObjectId { get; set; }
        public int UserId { get; set; }
        public long TimeStamp { get; set; }
    }
    public enum HeartbeatObjType
    {
        None = 0,
        TextSection = 1,
        FreeTable = 2
    }
}
