using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities
{
    public interface IPathable
    {
        public int Id { get; }
        public string? Name { get; }
        public int Depth { get; }
        public int ParentDir { get; }
    }
}
