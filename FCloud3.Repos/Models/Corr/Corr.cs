using FCloud3.Repos.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Corr
{
    public class Corr : IDbModel
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int Weight { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public enum CorrType
    {
        User_UserGroup = 10,
        User_Follow_User = 11,

        WikiItem_WikiGroup = 20
    }
    public class CorrRepo : RepoBase<Corr>
    {
        public CorrRepo(FCloudContext context) : base(context)
        {
        }
    }
}
