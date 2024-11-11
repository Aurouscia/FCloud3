using System.Net.Mime;
using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Services.Etc.Split;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace FCloud3.App.Controllers.Etc.Split
{
    public class SplitController : Controller
    {
        private readonly SplitService splitService;
        private readonly FCloudContext ctx;
        public SplitController(SplitService splitService, FCloudContext ctx)
        {
            this.splitService = splitService;
            this.ctx = ctx;
            if ("666".Any())
            {
                throw new InvalidOperationException("请注释throw语句以使用功能");
            }
        }

        [Route("/S/R/{dirId}")]
        public IActionResult GetReport(int dirId)
        {
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
        [Route("/S/KeepWiki/{dirId}")]
        public IActionResult DelWikiOutside(int dirId)
        {
            var res = splitService.AllWikiInDirReport(dirId);
            var keepIds = res.ConvertAll(x => x.Id);
            ctx.WikiItems.Where(x => !keepIds.Contains(x.Id)).ExecuteDelete();
            return Ok(keepIds.Count);
        }

        [Route("/S/KeepDir/{dirId}")]
        public IActionResult DelOtherDirs(int dirId)
        {
            var keepIds = splitService.DirDescendants(dirId);
            ctx.FileDirs.Where(x => !keepIds.Contains(x.Id)).ExecuteDelete();
            return Ok(keepIds.Count);
        }
        [Route("/S/DelWiki/{dirId}")]
        public IActionResult DelWikiInside(int dirId)
        {
            var res = splitService.AllWikiInDirReport(dirId);
            var dels = res.Where(x => !x.ExistsInOtherDir).ToList();
            var delIds = dels.Select(x => x.Id);
            DateTime now = DateTime.Now;
            var updated = ctx.WikiItems.Where(x => delIds.Contains(x.Id))
                .ExecuteUpdate(spc => spc
                    .SetProperty(x => x.Deleted, true)
                    .SetProperty(x => x.Updated, now)
                );
            return Ok(updated);
        }

        [Route("/S/DelDir/{dirId}")]
        public IActionResult DelDirs(int dirId)
        {
            var delIds = splitService.DirDescendants(dirId);
            ctx.FileDirs.Where(x => delIds.Contains(x.Id)).ExecuteDelete();
            return Ok(delIds.Count);
        }

        [Route("/S/DU/{groupId}")]
        public IActionResult DelUserOutside(int groupId)
        {
            var uids = ctx.UserToGroups.Where(x => x.GroupId == groupId).Select(x => x.UserId);
            ctx.Users.Where(x => !uids.Contains(x.Id) && x.Name != "Au")
                .ExecuteUpdate(spc => spc.SetProperty(u => u.Deleted, true));
            ctx.UserGroups.Where(x => x.Id != groupId)
                .ExecuteUpdate(spc => spc.SetProperty(u => u.Deleted, true));
            return Ok("Ok");
        }

        [Route("/S/RemoveRubbishParaAndFile")]
        public IActionResult RemoveRubbishParaAndFile()
        {
            var allWikiIds = ctx.WikiItems.Select(x => x.Id).ToHashSet();
            var allParas = ctx.WikiParas.Select(x => new { ParaId = x.Id, x.WikiItemId }).ToList();
            var keepParas = allParas.Where(x => allWikiIds.Contains(x.WikiItemId))
                .Select(x => x.ParaId).ToList();
            ctx.WikiParas.Where(x => !keepParas.Contains(x.Id)).ExecuteDelete();

            var allUserIds = ctx.Users.Where(x => !x.Deleted).Select(x => x.Id).ToHashSet();
            var allFileParasFileIds = ctx.WikiParas
                .Where(x => x.Type == WikiParaType.File)
                .Select(x => x.ObjectId).ToHashSet();
            var allFiles = ctx.FileItems.Select(x => new { FileId = x.Id, x.CreatorUserId }).ToList();
            var keepFiles = allFiles.Where(x => allFileParasFileIds.Contains(x.FileId)
                || allUserIds.Contains(x.CreatorUserId))
                .Select(x => x.FileId).ToList();
            ctx.FileItems.Where(x => !keepFiles.Contains(x.Id)).ExecuteDelete();
            return Ok($"移除垃圾段落{allParas.Count - keepParas.Count}个，" +
                $"移除垃圾文件{allFiles.Count - keepFiles.Count}个");
        }
    }
}