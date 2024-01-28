using Microsoft.AspNetCore.Mvc;
using FCloud3.App.Utils;

namespace FCloud3.App.Controllers.Sys
{
    public class UtilsController:Controller
    {
        public IActionResult UrlPathName(string input)
        {
            var res = PinYinHelper.ToUrlName(input);
            return this.ApiResp(new { res });
        }
    }
}
