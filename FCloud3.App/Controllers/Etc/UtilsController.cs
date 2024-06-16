using FCloud3.App.Models.Etc;
using Microsoft.AspNetCore.Mvc;
using FCloud3.App.Utils;

namespace FCloud3.App.Controllers.Etc
{
    public class UtilsController : Controller
    {
        private readonly IConfiguration _config;
        public UtilsController(IConfiguration config)
        {
            _config = config;
        }
        public IActionResult UrlPathName(string input)
        {
            var res = PinYinHelper.ToUrlName(input);
            return this.ApiResp(new { res });
        }
        public IActionResult ApplyBeingMember()
        {
            var res = _config["ApplyBeingMember"] ?? "暂不提供申请方式";
            return this.ApiResp(new { res });
        }
        public IActionResult GetFooterLinks()
        {
            var model = new FooterLinks();
            var footerLinks = _config.GetSection("FooterLinks").GetChildren();
            var dict = new List<(int, FooterLinks.FooterLink)>();
            foreach (var link in footerLinks)
            {
                string key = link.Key;
                string[] parts = key.Split('-');
                string text = key;
                int order = 0;
                if (parts.Length == 2)
                {
                    _ = int.TryParse(parts[0], out order);
                    text = parts[1];
                }
                dict.Add((order,new(text, link.Value)));
            }
            var links = dict.OrderBy(x => x.Item1).Select(x => x.Item2);
            model.Links.AddRange(links);
            return this.ApiResp(model);
        }
    }
}
