using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.Entities.Identities;
using FCloud3.Services.Etc.Split;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.App.Controllers.Etc.Split
{
    [Authorize]
    public class WikiImportExportController(
        WikiImportExportService wikiImportExportService,
        HttpUserInfoService userInfoService)
        : Controller
    {
        [RateLimited(60000, 1)]
        public IActionResult ExportMyWikis()
        {
            var userId = userInfoService.Id;
            if(userId <= 0)
                return BadRequest();
            var memStream = new MemoryStream();
            wikiImportExportService.ExportMyWikis(memStream, userId);
            memStream.Flush();
            memStream.Position = 0;
            return File(memStream, Application.Octet);
        }
        
        [UserTypeRestricted(UserType.SuperAdmin)]
        [RateLimited(60000, 1)]
        public IActionResult ExportAllWikis()
        {
            var memStream = new MemoryStream();
            wikiImportExportService.ExportMyWikis(memStream, 0);
            memStream.Flush();
            memStream.Position = 0;
            return File(memStream, Application.Octet);
        }

        [UserTypeRestricted(UserType.SuperAdmin)]
        [RateLimited(60000, 5)]
        public IActionResult PreviewImport(IFormFile file)
        {
            var userId = userInfoService.Id;
            if (userId <= 0)
                return BadRequest();
            if (file is null || file.Length == 0)
                return this.ApiFailedResp("请上传文件");
            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return this.ApiFailedResp("请上传zip格式的压缩包");

            using var stream = file.OpenReadStream();
            var preview = wikiImportExportService.PreviewImport(stream, out var errmsg);
            if (preview is not null)
                return this.ApiResp(preview);
            return this.ApiFailedResp(errmsg ?? "预览失败");
        }

        [UserTypeRestricted(UserType.SuperAdmin)]
        [RateLimited(60000, 5)]
        public IActionResult CheckFileStatus([FromBody] List<string> urls)
        {
            if (urls is null || urls.Count == 0)
                return this.ApiFailedResp("请提供文件URL列表");
            var results = wikiImportExportService.CheckFileStatus(urls);
            return this.ApiResp(results);
        }

        [UserTypeRestricted(UserType.SuperAdmin)]
        [RateLimited(60000, 1)]
        public IActionResult ImportWikis(IFormFile file)
        {
            var userId = userInfoService.Id;
            if (userId <= 0)
                return BadRequest();
            if (file is null || file.Length == 0)
                return this.ApiFailedResp("请上传文件");
            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return this.ApiFailedResp("请上传zip格式的压缩包");

            using var stream = file.OpenReadStream();
            var count = wikiImportExportService.ImportWikis(stream, userId, out var errmsg);
            if (count > 0)
                return this.ApiResp(new { ImportedCount = count });
            return this.ApiFailedResp(errmsg ?? "导入失败");
        }
    }
}
