using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.Files
{
    public class FileItem: IDbModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 展示的名称，例如"index.html"，可以重复
        /// </summary>
        [MaxLength(displayNameMaxLength)]
        public string? DisplayName { get; set; }
        /// <summary>
        /// 存储的路径(包括本身的文件名)，例如"wikiFile/666.png"，全局唯一
        /// </summary>
        [MaxLength(storePathNameMaxLength)]
        public string? StorePathName { get; set; }
        public int ByteCount { get; set; }
        [MaxLength(32)]
        public string? Hash { get; set; }
        public int InDir { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int displayNameMaxLength = 30;
        public const int storePathNameMaxLength = 50;
    }
}
