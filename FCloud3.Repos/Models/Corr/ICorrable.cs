using FCloud3.Entities.DbModels.Corr;

namespace FCloud3.Repos.Models.Cor
{
    public interface ICorrable
    {
        public bool MatchedCorr(Corr corr);
    }
}
