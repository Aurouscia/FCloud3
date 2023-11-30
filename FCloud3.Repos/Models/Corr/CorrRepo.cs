using FCloud3.Entities.DbModels.Corr;
using FCloud3.Repos.DbContexts;

namespace FCloud3.Repos.Models.Cor
{
    public class CorrRepo : RepoBase<Corr>
    {
        public CorrRepo(FCloudContext context) : base(context)
        {
        }
    }
}
