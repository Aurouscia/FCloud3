using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Messages
{
    public class Comment : IDbModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 评论的对象类型
        /// </summary>
        public CommentTargetType TargetType { get; set; }
        /// <summary>
        /// 评论的对象Id
        /// </summary>
        public int TargetObjId { get; set; }
        /// <summary>
        /// 该评论回复的评论（如果有）的id
        /// </summary>
        public int ReplyingTo { get; set; }
        /// <summary>
        /// 对评论对象的打分（1-10）
        /// </summary>
        public byte Rate { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        [MaxLength(contentMaxLength)]
        public string? Content { get; set; }
        public int HiddenByUser { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public const int contentMaxLength = 256;
    }

    public enum CommentTargetType
    {
        None = 0,
        Wiki = 1,
    }
}
