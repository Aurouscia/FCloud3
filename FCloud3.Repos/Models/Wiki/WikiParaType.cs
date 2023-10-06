using FCloud3.Repos.Models.Corr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Wiki
{
    public enum WikiParaType
    {
        Text = 0,
        File = 1,
        Table = 2 
    }
    public static class WikiParaTypeUtil
    {
        public static List<CorrType> WikiParaCorrTypes {
            get
            {
                return new()
                {
                    CorrType.WikiTextPara_WikiItem,
                    CorrType.WikiFilePara_WikiItem,
                    CorrType.WikiTablePara_WikiItem
                };
            }
        } 
        public static WikiParaType ToWikiPara(this CorrType corr)
        {
            if(corr == CorrType.WikiTextPara_WikiItem)
            {
                return WikiParaType.Text;
            }
            else if(corr == CorrType.WikiFilePara_WikiItem)
            {
                return WikiParaType.File;
            }
            else if(corr == CorrType.WikiTablePara_WikiItem)
            {
                return WikiParaType.Table;
            }
            throw new NotImplementedException();
        }
        public static CorrType ToCorrType(this WikiParaType para)
        {
            if (para == WikiParaType.Text)
            {
                return CorrType.WikiTextPara_WikiItem;
            }
            else if (para == WikiParaType.File)
            {
                return CorrType.WikiFilePara_WikiItem;
            }
            else if (para == WikiParaType.Table)
            {
                return CorrType.WikiTablePara_WikiItem;
            }
            throw new NotImplementedException();
        }
    }
}
