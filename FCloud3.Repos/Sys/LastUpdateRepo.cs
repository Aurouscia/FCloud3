using FCloud3.DbContexts;
using FCloud3.Entities.Sys;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;
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
            => LastUpdateDbUtil.SetLastUpdateFor(ctx, type, time);
        public DateTime GetLastUpdateFor(LastUpdateType type, Func<DateTime>? init = null)
            => LastUpdateDbUtil.GetLastUpdateFor(ctx, type, init);
        public DateTime GetLastUpdateFor(LastUpdateType[] types, Func<DateTime>? init = null)
            => LastUpdateDbUtil.GetLastUpdateFor(ctx, types, init);
    }

    public static class LastUpdateDbUtil
    {
        private static readonly DateTime defaultTime = new(1900, 1, 1);
        private static readonly object lockObj = new();
        public static void SetLastUpdateFor(FCloudContext ctx, LastUpdateType type, DateTime time)
        {
            lock (lockObj)
            {
                var updated = ctx.LastUpdates
                    .Where(x => x.Type == type)
                    .ExecuteUpdate(spc => spc
                        .SetProperty(x => x.LastUpdateTime, time));
                if (updated == 0)
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
        }
        public static void SetLastUpdateFor(FCloudContext ctx, LastUpdateType[] types, DateTime time)
        {
            lock (lockObj)
            {
                var updated = ctx.LastUpdates
                    .Where(x => types.Contains(x.Type))
                    .ExecuteUpdate(spc => spc
                        .SetProperty(x => x.LastUpdateTime, time));
                if (updated < types.Length)
                {
                    var existingTypes = ctx.LastUpdates
                        .Where(x => types.Contains(x.Type))
                        .Select(x => x.Type).ToList();
                    List<LastUpdate> models = [];
                    foreach (var t in types)
                    {
                        if (existingTypes.Contains(t))
                            continue;
                        models.Add(new()
                        {
                            Type = t,
                            LastUpdateTime = time
                        });
                    };
                    if (models.Count > 0)
                    {
                        ctx.LastUpdates.AddRange(models);
                        ctx.SaveChanges();
                    }
                }
            }
        }
        public static DateTime GetLastUpdateFor(FCloudContext ctx, LastUpdateType type, Func<DateTime>? init = null)
        {
            lock (lockObj)
            {
                var res = ctx.LastUpdates
                    .Where(x => x.Type == type)
                    .FirstOrDefault();
                if (res is null)
                {
                    var initVal = init is { } ? init() : defaultTime;
                    LastUpdate model = new()
                    {
                        Type = type,
                        LastUpdateTime = initVal
                    };
                    ctx.LastUpdates.Add(model);
                    ctx.SaveChanges();
                    return initVal;
                }
                return res.LastUpdateTime;
            }
        }
        public static DateTime GetLastUpdateFor(FCloudContext ctx, LastUpdateType[] types, Func<DateTime>? init = null)
        {
            lock (lockObj)
            {
                // .net10中，enum数组不能使用Contains方法，需要使用Any方法
                var res = ctx.LastUpdates
                    .Where(x => types.Any(t => t == x.Type))
                    .ToList();
                if (res.Count > 0)
                    return res.Select(x => x.LastUpdateTime).Max();
                else
                {
                    var existingTypes = res.Select(x => x.Type);
                    var initVal = init is { } ? init() : defaultTime;
                    List<LastUpdate> models = [];
                    foreach (var t in types)
                    {
                        if (existingTypes.Contains(t))
                            continue;
                        models.Add(new()
                        {
                            Type = t,
                            LastUpdateTime = initVal
                        });
                    };
                    if (models.Count > 0)
                    {
                        ctx.LastUpdates.AddRange(models);
                        ctx.SaveChanges();
                    }
                    return initVal;
                }
            }
        }
    }
}
