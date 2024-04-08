using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Diff
{
    public class DiffContent : IDbModel
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public DiffContentType DiffType { get; set; }
        public int RemovedChars { get; set; }
        public int AddedChars { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public enum DiffContentType
    {
        None = 0,
        TextSection = 1,
        FreeTable = 2
    }
}
