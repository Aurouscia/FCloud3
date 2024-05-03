using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class LatestWorkController : Controller
    {
        private readonly LatestWorkService _latestWorkService;

        public LatestWorkController(LatestWorkService latestWorkService)
        {
            _latestWorkService = latestWorkService;
        }

        public IActionResult Get(int uid = -1)
        {
            return this.ApiResp(_latestWorkService.Get(uid));
        }
    }
}
