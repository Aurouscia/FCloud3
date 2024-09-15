using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class GuideInfoController(IConfiguration config) : Controller
    {
        public IActionResult CreateWiki()
        {
            string? g = config["Guides:CreateWiki"];
            return this.ApiResp(new GuideInfoResponse(g));
        }

        public IActionResult SiteIntro()
        {
            string? i = config["Guides:SiteIntro"];
            return this.ApiResp(new GuideInfoResponse(i));
        }
        
        public IActionResult SiteRegulations()
        {
            string? r = config["Guides:SiteRegulations"];
            return this.ApiResp(new GuideInfoResponse(r));
        }
    }

    public class GuideInfoResponse(string? text)
    {
        public string? Text { get; set; } = text;
    }
}