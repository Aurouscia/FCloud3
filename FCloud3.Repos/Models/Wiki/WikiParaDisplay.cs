using FCloud3.Entities.DbModels.Corr;
using FCloud3.Repos.Models.Cor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiParaDisplay
    {
        /// <summary>
        /// 指该段落与本词条对应的corr的Id
        /// </summary>
        public int CorrId { get; set; }
        /// <summary>
        /// 指该段落代表的文本段/文件/表格的Id
        /// </summary>
        public int UnderlyingId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        /// <summary>
        /// 指该段落与本词条对应的corr的Order
        /// </summary>
        public int Order { get; set; }
        public WikiParaType Type { get; set; }
        public WikiParaDisplay(Corr corr, int underlyingId, string? title,string? content,WikiParaType type)
        {
            CorrId = corr.Id;
            Order = corr.Order;
            UnderlyingId = underlyingId;
            Title = title;
            Content = content;
            Type = type;
        }
    }
    public static class WikiParaDisplayListConverter
    {
        public static List<WikiParaDisplay> ToDisplaySimpleList(this List<KeyValuePair<Corr, IWikiPara>> para)
        {
            return para.ConvertAll(x =>
            {
                Corr paraCorr = x.Key;
                IWikiPara paraEntity = x.Value;
                WikiParaDisplay display = paraEntity.ToDisplaySimple(paraCorr);
                return display;
            });
        }
    }
    public class WikiParaPlaceholder : IWikiPara
    {
        public WikiParaType Type { get; set; }
        public WikiParaPlaceholder(WikiParaType type)
        {
            Type = type;
        }

        public WikiParaDisplay ToDisplay(Corr corr)
        {
            return new WikiParaDisplay(corr,0 ,"新段落","",Type);
        }

        public WikiParaDisplay ToDisplaySimple(Corr corr)
        {
            return new WikiParaDisplay(corr,0, "新段落", "", Type);
        }
    }
}
