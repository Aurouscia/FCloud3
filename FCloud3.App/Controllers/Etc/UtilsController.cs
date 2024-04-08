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
    }
}
