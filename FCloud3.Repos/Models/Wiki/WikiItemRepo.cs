using FCloud3.Entities.DbModels.Wiki;
using FCloud3.DbContexts;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiItemRepo : RepoBase<WikiItem>
    {
        public WikiItemRepo(FCloudContext context) : base(context)
        {
        }
    }
}
