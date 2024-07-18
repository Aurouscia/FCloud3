using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Messages
{
    public class OpRecord : IDbModel
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public OpRecordOpType OpType { get; set; }
        public OpRecordTargetType TargetType { get; set; }
        /// <summary>
        /// 主Id（例如“为目录加入词条”的目录Id），对应当前操作目标（TargetType）
        /// </summary>
        public int ObjA { get; set; }
        /// <summary>
        /// 副Id（例如“为目录加入词条”的词条Id）
        /// </summary>
        public int ObjB { get; set; }
        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public enum OpRecordOpType
    {
        None = 0,
        Create = 10,
        Edit = 20,
        EditImportant = 21,
        Remove = 30,
    }
    public enum OpRecordTargetType
    {
        None = 0,
        WikiItem = 10,
        WikiPara = 11,
        TextSection = 20,
        FreeTable = 21,
        FileDir = 30,
        FileItem = 31,
        UserGroup = 40,
        User = 41
    }
}
