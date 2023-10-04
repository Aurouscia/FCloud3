using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Wiki
{
    public interface IWikiPara
    {
        public WikiParaDisplay ToDisplay();
        public WikiParaDisplay ToDisplaySimple();
    }
}
