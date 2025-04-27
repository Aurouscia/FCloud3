

using System.ComponentModel.DataAnnotations;

namespace FCloud3.Entities.Wiki
{
    /// <summary>
    /// 表示一个可以作为Wiki段落的对象<br/>
    /// 原计划：词条与文本/表格可以多对多关系<br/>
    /// 已放弃：无法解决词条导出/导入、权限控制等问题<br/>
    /// 替代方案：AuParaLoader插件
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
        /// <summary>
        /// 如果不为null，覆盖段落名称
        /// </summary>
        [MaxLength(nameOverrideMaxLength)]
        public string? NameOverride { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public int RelationMainId => WikiItemId;
        public int RelationSubId => ObjectId;
        public const int nameOverrideMaxLength = 32;
        public const string copyableMark = "（可复制）";
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
        public static string Readable(WikiParaType type)
        {
            if (type == WikiParaType.Text)
                return "文本";
            else if (type == WikiParaType.File)
                return "文件";
            else if (type == WikiParaType.Table)
                return "表格";
            return "未知";
        }
    }
}
