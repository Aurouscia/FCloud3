using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Wiki
{
    public class WikiTitleContain : IDbModel
    {
        public int Id { get; set; }
        public int WikiId { get; set; }
        public WikiTitleContainType Type { get; set; }
        public int ObjectId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        /// <summary>
        /// 在 WikiTitleContain 的上下文中，Deleted表示被用户加入黑名单，
        /// 自动填充将忽略Deleted为true的项，但手动重新添加会将其设回false，
        /// 所以任何时候都不应该彻底删除Deleted项目
        /// </summary>
        public bool Deleted { get; set; }
    }
    public enum WikiTitleContainType
    {
        Unknown = 0,
        TextSection = 1,
        FreeTable = 2,
    }
}
