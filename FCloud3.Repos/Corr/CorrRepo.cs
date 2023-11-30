using FCloud3.DbContexts;
using FCloud3.Entities.Corr;

namespace FCloud3.Repos.Cor
{
    public class CorrRepo : RepoBase<Corr>
    {
        public CorrRepo(FCloudContext context) : base(context)
        {
        }
    }
}
