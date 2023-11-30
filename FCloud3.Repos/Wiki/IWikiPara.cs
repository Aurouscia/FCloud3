using FCloud3.Entities.Corr;

namespace FCloud3.Repos.Wiki
{
    public interface IWikiPara
    {
        public WikiParaDisplay ToDisplay(Corr corrWithCurrentWiki);
        public WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki);
    }

    public abstract class WikiPara : Corr, IWikiPara
    {
        public WikiParaType ParaType => CorrType.ToWikiPara();
        public abstract WikiParaDisplay ToDisplay(Corr corrWithCurrentWiki);
        public abstract WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki);
    }
}
