using FCloud3.DbContexts;
using FCloud3.Entities.Sys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Sys
{
    public class LastUpdateRepo(FCloudContext ctx)
    {
        public void SetLastUpdateFor(LastUpdateType type, DateTime time)
        {
            var updated = ctx.LastUpdates
                .Where(x => x.Type == type)
                .ExecuteUpdate(spc => spc
                    .SetProperty(x => x.LastUpdateTime, time));
            if(updated == 0)
            {
                LastUpdate model = new()
                {
                    Type = type,
                    LastUpdateTime = time
                };
                ctx.LastUpdates.Add(model);
                ctx.SaveChanges();
            }
        }
        public DateTime GetLastUpdateFor(LastUpdateType type, Func<DateTime> init)
        {
            var res = ctx.LastUpdates
                .Where(x => x.Type == type)
                .Select(x => x.LastUpdateTime)
                .FirstOrDefault();
            if(res == default)
            {
                var initVal = init();
                LastUpdate model = new()
                {
                    Type = type,
                    LastUpdateTime = initVal
                };
                ctx.LastUpdates.Add(model);
                ctx.SaveChanges();
                return initVal;
            }
            return res;
        }
    }
}
