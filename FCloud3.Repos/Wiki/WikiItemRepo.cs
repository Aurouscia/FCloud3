using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;

namespace FCloud3.Repos.Wiki
{
    public class WikiItemRepo : RepoBase<WikiItem>
    {
        public WikiItemRepo(FCloudContext context) : base(context)
        {
        }
    }
}
