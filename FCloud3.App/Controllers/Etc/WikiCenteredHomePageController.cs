using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class WikiCenteredHomePageController(WikiCenteredHomePageService service) : Controller
    {
        private readonly WikiCenteredHomePageService _service = service;
        public IActionResult Get(int latestCount)
        {
            return this.ApiResp(_service.Get(latestCount));
        }
    }
}