using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Files
{
    public class FileDir: IDbModel, IPathable
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? UrlPathName { get; set; }
        public int ParentDir {  get; set; }
        public int RootDir { get; set; }
        public int Depth { get; set; }
        public int ByteCount { get; set; }
        public int ContentCount { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public override string ToString()
        {
            return $"Id:{Id} 父级:{ParentDir}";
        }
    }
}
