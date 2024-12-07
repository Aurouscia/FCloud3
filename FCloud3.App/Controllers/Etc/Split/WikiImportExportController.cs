using FCloud3.App.Services.Filters;
using FCloud3.Services.Etc.Split;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.App.Controllers.Etc.Split
{
    [Authorize]
    public class WikiImportExportController(
        WikiImportExportService wikiImportExportService)
        : Controller
    {
        [RateLimited(60000, 1)]
        public IActionResult ExportMyWikis()
        {
            var memStream = new MemoryStream();
            wikiImportExportService.ExportMyWikis(memStream);
            memStream.Flush();
            memStream.Position = 0;
            return File(memStream, Application.Octet);
        }
    }
}
