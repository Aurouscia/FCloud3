using FCloud3.DbContexts;
using FCloud3.Entities;
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
            return _context.Set<T>()
                .Where(x=>x.Id==modelId)
                .Select(x=>x.CreatorUserId)
                .FirstOrDefault();
        }
    }
}
