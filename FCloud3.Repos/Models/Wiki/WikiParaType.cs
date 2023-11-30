using FCloud3.Entities.DbModels.Corr;
using FCloud3.Repos.Models.Cor;
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
                    CorrType.TextSection_WikiItem,
                    CorrType.FileItem_WikiItem,
                    CorrType.TableItem_WikiItem
                };
            }
        } 
        public static WikiParaType ToWikiPara(this CorrType corr)
        {
            if(corr == CorrType.TextSection_WikiItem)
            {
                return WikiParaType.Text;
            }
            else if(corr == CorrType.FileItem_WikiItem)
            {
                return WikiParaType.File;
            }
            else if(corr == CorrType.TableItem_WikiItem)
            {
                return WikiParaType.Table;
            }
            throw new NotImplementedException();
        }
        public static CorrType ToCorrType(this WikiParaType para)
        {
            if (para == WikiParaType.Text)
            {
                return CorrType.TextSection_WikiItem;
            }
            else if (para == WikiParaType.File)
            {
                return CorrType.FileItem_WikiItem;
            }
            else if (para == WikiParaType.Table)
            {
                return CorrType.TableItem_WikiItem;
            }
            throw new NotImplementedException();
        }
    }
}
