using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Entities.Table
{
    [Index(nameof(CreatorUserId))]
    public class FreeTable : IDbModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Data { get; set; }
        public string? Brief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
