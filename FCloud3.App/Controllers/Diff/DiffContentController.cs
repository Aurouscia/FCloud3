using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.Entities.Diff;
using FCloud3.Entities.Identities;
using FCloud3.Services.Diff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Diff
{
    public class DiffContentController(
        DiffContentService diffContentService,
        HttpUserInfoService httpUserInfoService) : Controller
    {
        private readonly DiffContentService _diffContentService = diffContentService;
        private readonly HttpUserInfoService _httpUserInfoService = httpUserInfoService;

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
            var isAdmin = _httpUserInfoService.IsAdmin;
            var res = _diffContentService.DiffDetail(diffId, out var errmsg, isAdmin);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }
        public IActionResult DetailExact(int diffId)
        {
            var res = _diffContentService.DiffDetail(diffId, out var errmsg, true, true);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }

        [Authorize]
        [UserTypeRestricted(UserType.Admin)]
        public IActionResult SetHidden(int diffId, bool hidden)
        {
            var res = _diffContentService.SetHidden(diffId, hidden, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult GetDiffHistoriesByDays(int days)
        {
            days = Math.Clamp(days, 0, 7);
            var res = _diffContentService.GetDiffHistoriesByDays(days, out string? errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }
    }
}
