using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos.Wiki
{
    public class WikiRefRepo(FCloudContext ctx)
    {
        public DbSet<WikiRef> WikiRefs { get; } = ctx.WikiRefs;
        public void SetRefs(int wikiId, HashSet<string> refs)
        {
            var existing = WikiRefs.Where(x => x.WikiId == wikiId).ToList();
            var removing = existing.Where(x => x.Str is null || !refs.Contains(x.Str));
            var addingStrs = refs.Except(removing.Select(x => x.Str));
            var adding = addingStrs.Select(x => new WikiRef()
            {
                WikiId = wikiId,
                Str = x
            });
            WikiRefs.AddRange(adding);
            WikiRefs.RemoveRange(removing);
            ctx.SaveChanges();
        }
    }
}
