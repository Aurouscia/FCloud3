using FCloud3.Entities.Diff;
using FCloud3.Services.Diff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Diff
{
    [Authorize]
    public class DiffContentController : Controller
    {
        private readonly DiffContentService _diffContentService;

        public DiffContentController(DiffContentService diffContentService)
        {
            _diffContentService = diffContentService;
        }

        public IActionResult History(DiffContentType type, int objId)
        {
            return this.ApiResp(_diffContentService.DiffHistory(type, objId));
        }

        public IActionResult Detail(DiffContentType type, int objId)
        {
            return this.ApiResp(_diffContentService.DiffDetail(type, objId));
        }
    }
}
