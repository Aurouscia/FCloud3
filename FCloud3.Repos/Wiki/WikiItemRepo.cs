using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;

namespace FCloud3.Repos.Wiki
{
    public class WikiItemRepo : RepoBase<WikiItem>
    {
        public WikiItemRepo(FCloudContext context) : base(context)
        {
        }

        public IQueryable<WikiItem> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Title != null && x.Title.Contains(str))
                .OrderBy(x => x.Updated);//可能要按什么别的办法排序
        }
    }
}
