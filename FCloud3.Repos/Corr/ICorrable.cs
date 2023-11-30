using FCloud3.Entities.Corr;

namespace FCloud3.Repos.Cor
{
    public interface ICorrable
    {
        public bool MatchedCorr(Corr corr);
    }
}
