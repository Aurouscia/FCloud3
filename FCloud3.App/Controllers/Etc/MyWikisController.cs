using FCloud3.App.Services.Utils;
using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    [Authorize]
    public class MyWikisController(
        MyWikisService myWikisService,
        HttpUserIdProvider userIdProvider)
        :Controller
    {
        public IActionResult MyWikisOverall(int uid)
        {
            if (uid == 0)
                uid = userIdProvider.Get();
            return this.ApiResp(myWikisService.MyWikiOverall(uid));
        }
    }
}