using FCloud3.Repos.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiPara : IDbModel
    {
        public int Id { get;set; }
        public string? Title { get; set; }
        public string? Content { get; set; }

        public int CreatorUserId { get;set; }
        public DateTime Created { get;set; }
        public DateTime Updated { get;set; }
        public bool Deleted { get;set; }
    }
}
