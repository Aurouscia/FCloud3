using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos
{
    public class CreatorIdGetter
    {
        private readonly FCloudContext _context;

        public CreatorIdGetter(FCloudContext context)
        {
            _context = context;
        }
        public int Get<T>(int modelId) where T : class, IDbModel
        {
            if(typeof(T) == typeof(WikiItem))
            {
                return _context.Set<WikiItem>()
                    .Where(x => x.Id == modelId)
                    .Select(x => x.OwnerUserId)
                    .FirstOrDefault();
            }
            if (typeof(T) == typeof(UserGroup))
            {
                return _context.Set<UserGroup>()
                    .Where(x => x.Id == modelId)
                    .Select(x => x.OwnerUserId)
                    .FirstOrDefault();
            }
            return _context.Set<T>()
                .Where(x=>x.Id==modelId)
                .Select(x=>x.CreatorUserId)
                .FirstOrDefault();
        }
    }
}
