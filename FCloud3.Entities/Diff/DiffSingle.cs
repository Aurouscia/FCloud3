using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Diff
{
    public class DiffSingle
    {
        public int Id { get; set; }
        public int DiffContentId { get; set; }
        public int Index { get; set; }
        public string? Ori { get; set; }
        public int New { get; set; }
    }
}
