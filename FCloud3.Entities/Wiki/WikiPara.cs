using FCloud3.Entities.Wiki.Paragraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Wiki
{
    /// <summary>
    /// 表示一个可以作为Wiki段落的对象
    /// </summary>
    public class WikiPara : IDbModel, IRelation
    {
        public int Id { get; set; }

        /// <summary>
        /// 所属的Wiki词条Id
        /// </summary>
        public int WikiItemId { get; set; }
        /// <summary>
        /// 连接的对象(可能是文本段，可能是表格，可能是文件)的Id
        /// </summary>
        public int ObjectId { get; set; }
        /// <summary>
        /// wiki段落类型
        /// </summary>
        public WikiParaType Type { get; set; }
        /// <summary>
        /// 段落在词条内的顺序号
        /// </summary>
        public int Order { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public int RelationMainId => WikiItemId;
        public int RelationSubId => ObjectId;
    }

    public enum WikiParaType
    {
        Text = 0,
        File = 1,
        Table = 2
    }

    public static class WikiParaTypes
    {
        public static List<WikiParaType> GetListInstance()
        {
            return new List<WikiParaType>()
            {
                WikiParaType.Text,
                WikiParaType.File,
                WikiParaType.Table
            };
        }
    }
}
