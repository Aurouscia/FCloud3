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
            var res = _diffContentService.DiffHistory(type, objId, out var errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }
        
        public IActionResult HistoryForWiki(string wikiPathName)
        {
            var res = _diffContentService.DiffHistoryForWiki(wikiPathName, out var errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }

        public IActionResult Detail(int diffId)
        {
            var res = _diffContentService.DiffDetail(diffId, out var errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }
    }
}
