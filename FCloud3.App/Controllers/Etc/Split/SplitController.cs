using System.Net.Mime;
using FCloud3.Services.Etc.Split;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc.Split
{
    public class SplitController(
        SplitService splitService) : Controller
    {
        [Route("/S/R/{dirId}")]
        public IActionResult GetReport(int dirId)
        {
            if ("666".Any())
            {
                throw new InvalidOperationException("请注释throw语句以使用功能");
            }
            var res = splitService.AllWikiInDirReport(dirId);
            res.Sort((x, y) 
                => string.Compare(x.Title, y.Title, StringComparison.Ordinal));
            List<AllWikiInDirReportItem> nonExclusive = [];
            List<AllWikiInDirReportItem> exclusive = [];
            res.ForEach(w =>
            {
                if(w.ExistsInOtherDir)
                    nonExclusive.Add(w);
                else
                    exclusive.Add(w);
            });
            MemoryStream ms = new();
            StreamWriter sw = new(ms);
            sw.WriteLine("【目录独占词条（不在原处保留）】");
            exclusive.ForEach(w =>
            {
                sw.Write("                                        ");
                sw.WriteLine(w.Owner);
                sw.WriteLine(w.Title);
            });
            sw.WriteLine("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
            sw.WriteLine("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
            sw.WriteLine("【非目录独占词条（会造成分离版本）】");
            nonExclusive.ForEach(w =>
            {
                sw.Write("                                        ");
                sw.WriteLine(w.Owner);
                sw.WriteLine(w.Title);
            });
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, MediaTypeNames.Application.Octet, $"目录{dirId}词条报告.txt");
        }
    }
}