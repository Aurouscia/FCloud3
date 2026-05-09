using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos.Etc
{
    public static class WikiItemQueryableExtension
    {
        public static IQueryable<WikiItem> HavingPara(this IQueryable<WikiItem> q, FCloudContext ctx)
        {
            return q.Join(
                ctx.WikiParas.Where(p => !p.Deleted).Select(p => p.WikiItemId).Distinct(),
                w => w.Id,
                wikiId => wikiId,
                (w, _) => w);
        }
    }
}
