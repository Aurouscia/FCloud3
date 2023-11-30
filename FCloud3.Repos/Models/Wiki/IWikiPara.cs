using FCloud3.Entities.DbModels.Corr;

namespace FCloud3.Repos.Models.Wiki
{
    public interface IWikiPara
    {
        public WikiParaDisplay ToDisplay(Corr corrWithCurrentWiki);
        public WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki);
    }

    public abstract class WikiPara : Corr, IWikiPara
    {
        public WikiParaType ParaType => base.CorrType.ToWikiPara();
        public abstract WikiParaDisplay ToDisplay(Corr corrWithCurrentWiki);
        public abstract WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki);
    }
}
