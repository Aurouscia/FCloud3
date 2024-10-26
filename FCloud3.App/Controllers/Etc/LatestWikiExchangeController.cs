using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class LatestWikiExchangeController(
        LatestWikiExchangeService latestWikiExchangeService) 
        : Controller
    {
        [Route(LatestWikiExchangeService.pushRoute)]
        public IActionResult Push()
        {
            return Ok("OK");
        }
    }
}