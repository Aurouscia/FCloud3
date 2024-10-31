using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files;
using FCloud3.WikiPreprocessor.DataSource;
using FCloud3.WikiPreprocessor.DataSource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.WikiParsing.Support
{
    public class WikiParserDataSource(
        WikiItemRepo wikiItemRepo,
        MaterialRepo materialRepo,
        MaterialService materialService) 
        : IScopedDataSource
    {
        public string? Implant(string implantSpan)
        {
            var spanTrimmed = implantSpan.Trim();
            var parts = spanTrimmed.Split(materialImplantSep);
            var firstPart = parts.ElementAtOrDefault(0);
            var secondPart = parts.ElementAtOrDefault(1);
            var m = materialRepo.CachedItemsByPred(m => m.Name == firstPart).FirstOrDefault();
            if (m is not null && m.Name != null && m.PathName != null)
                return MaterialReplacement(m.PathName, secondPart);

            var w = wikiItemRepo.CachedItemsByPred(w 
                => w.UrlPathName == spanTrimmed || w.Title == spanTrimmed)
                .FirstOrDefault();
            if (w is not null && w.UrlPathName != null && w.Title != null)
                return WikiReplacement(w.UrlPathName, w.Title);
            return null;
        }
        public LinkItem? Link(string linkSpan)
        {
            var w = wikiItemRepo.CachedItemByPred(x
                => x.UrlPathName == linkSpan || x.Title == linkSpan);
            if (w is { } && w.Title is { } && w.UrlPathName is { })
                return new(w.Title, w.UrlPathName);
            return null;
        }
        public string? Replace(string replaceTarget)
        {
            var w = wikiItemRepo
                .CachedItemsByPred(x => x.Title == replaceTarget)
                .FirstOrDefault();
            if (w != null && w.UrlPathName != null && w.Title != null)
                return WikiReplacement(w.UrlPathName, w.Title);
            return null;
        }

        public static string WikiReplacement(string urlPathName, string title) 
            => $"<a pathName=\"{urlPathName}\">{title}</a>";

        private static readonly char[] materialImplantSep = new[] { ':', '：' };
        private string MaterialReplacement(string pathName, string? heightExp)
        {
            if (heightExp is null)
                heightExp = "2rem";
            else if (float.TryParse(heightExp, out float _))
                heightExp += "rem";
            string src = MaterialSrc(pathName);
            return $"<img class=\"wikiInlineImg\" src=\"{src}\" style=\"height:{heightExp}\" />";
        }
        private string MaterialSrc(string pathName) 
            => materialService.GetMaterialFullSrc(pathName);
    }
}
