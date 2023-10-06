using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiParaDisplay
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Order { get; set; }
        public WikiParaType Type { get; set; }
    }
    public static class WikiParaDisplayListConverter
    {
        public static List<WikiParaDisplay> ToDisplaySimpleList(this List<KeyValuePair<Corr.Corr, IWikiPara>> para)
        {
            return para.ConvertAll(x =>
            {
                Corr.Corr paraCorr = x.Key;
                IWikiPara paraEntity = x.Value;
                WikiParaDisplay display = paraEntity.ToDisplaySimple();
                display.Order = paraCorr.Order;
                display.Id = paraCorr.Id;
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

        public WikiParaDisplay ToDisplay()
        {
            return new WikiParaDisplay()
            {
                Id = 0,
                Title = "新段落",
                Content = "",
                Type = this.Type
            };
        }

        public WikiParaDisplay ToDisplaySimple()
        {
            return new WikiParaDisplay()
            {
                Id = 0,
                Title = "新段落",
                Content = "",
                Type = this.Type
            };
        }
    }
}
