using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities.Identities
{
    public class UserGroup : IDbModel
    {
        public int Id { get; set; }
        [MaxLength(10)]
        public string? Name { get; set; }
        public int OwnerUserId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get;set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public static class UserGroupReservedNames
    {
        public static List<string> Get()
        {
            return new() { "管理员", "超级管理员" };
        }
    }
}
