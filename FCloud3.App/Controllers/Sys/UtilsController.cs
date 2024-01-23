using FCloud3.Utils.Utils.UrlPath;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Sys
{
    public class UtilsController:Controller
    {
        public IActionResult UrlPathName(string input)
        {
            var res = UrlPathNameUtil.ToUrlName(input);
            return this.ApiResp(new { res });
        }
    }
}
