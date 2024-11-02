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
        /// <summary>
        /// 用户的“上次活跃”时间，用于显示在页面上，区别于模型的更新时间
        /// </summary>
        public DateTime LastActive { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
        
        public const string defaultAvatar = "/defaultAvatar.svg";
    }

    public enum UserType
    {
        Tourist = 0,
        Member = 1,
        Admin = 8,
        SuperAdmin = 9
    }

    public static class UserTypes
    {
        public static string Readable(UserType type)
        {
            switch (type)
            {
                case UserType.Tourist:
                    return "游客";
                case UserType.Member:
                    return "会员";
                case UserType.Admin:
                    return "管理";
                case UserType.SuperAdmin:
                    return "超管";
                default:
                    return "??";
            }
        }
    }
}
