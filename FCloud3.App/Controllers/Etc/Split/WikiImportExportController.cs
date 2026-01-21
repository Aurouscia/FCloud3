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
    }
}
