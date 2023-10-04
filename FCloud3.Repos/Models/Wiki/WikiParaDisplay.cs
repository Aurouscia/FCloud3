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
        //有点问题，待优化
        public static List<WikiParaDisplay> ToDisplaySimpleList(this List<IWikiPara> para, List<Corr.Corr> corrs)
        {
            List<WikiParaDisplay> paras = new();
            for(int i = 0; i < para.Count; i++)
            {
                WikiParaDisplay display = para[i].ToDisplaySimple();
                display.Order = i;
                var c = corrs.Find(x => x.CorrType.ToWikiPara() == display.Type && x.Order == display.Order) 
                    ?? throw new Exception("未知错误");
                display.Id = c.Id;
                paras.Add(display);
            }
            return paras;
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
