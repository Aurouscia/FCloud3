using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Entities.Wiki
{
    [Index(nameof(WikiId))]
    [Index(nameof(ObjectId), nameof(Type))]
    public class WikiTitleContain : IDbModel
    {
        public int Id { get; set; }
        public int WikiId { get; set; }
        public WikiTitleContainType Type { get; set; }
        public int ObjectId { get; set; }
        public bool BlackListed {  get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public enum WikiTitleContainType
    {
        Unknown = 0,
        TextSection = 1,
        FreeTable = 2,
    }
}
