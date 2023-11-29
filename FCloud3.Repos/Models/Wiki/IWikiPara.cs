using FCloud3.Repos.Models.Cor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
