using FCloud3.DbContexts;
using FCloud3.Entities.DbModels.Corr;

namespace FCloud3.Repos.Models.Cor
{
    public class CorrRepo : RepoBase<Corr>
    {
        public CorrRepo(FCloudContext context) : base(context)
        {
        }
    }
}
