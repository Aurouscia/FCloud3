using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.App.Controllers.Etc
{
    public class LatestWikiExchangeController(
        LatestWikiExchangeService latestWikiExchangeService) 
        : Controller
    {
        [Route(LatestWikiExchangeService.pushRoute)]
        public IActionResult Push([FromBody]ExchangePushRequest req)
        {
            latestWikiExchangeService.BePushed(req);
            return Ok();
        }
        [Route(LatestWikiExchangeService.pullRoute)]
        public IActionResult Pull([FromBody] ExchangePullRequest req)
        {
            var res = latestWikiExchangeService.BePulled(req);
            var content = JsonConvert.SerializeObject(res);
            return Content(content, Application.Json);
        }
    }
}