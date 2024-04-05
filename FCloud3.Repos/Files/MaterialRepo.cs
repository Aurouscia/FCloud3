using FCloud3.DbContexts;
using FCloud3.Entities.Files;

namespace FCloud3.Repos.Files
{
    public class MaterialRepo : RepoBase<Material>
    {
        public MaterialRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        { }

        public IQueryable<Material> QuickSearch(string s)
        {
            return _context.Materials.Where(x => x.Name != null && x.Name.Contains(s));
        }
    }
}
