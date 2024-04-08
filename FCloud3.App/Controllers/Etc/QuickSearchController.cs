using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class QuickSearchController : Controller
    {
        private readonly QuickSearchService _quickSearchService;

        public QuickSearchController(QuickSearchService quickSearchService)
        {
            _quickSearchService = quickSearchService;
        }
        public IActionResult WikiItem(string s)
        {
            var res = _quickSearchService.SearchWikiItem(s);
            return this.ApiResp(res);
        }
        public IActionResult UserName(string s)
        {
            var res = _quickSearchService.SearchUser(s);
            return this.ApiResp(res);
        }
        public IActionResult UserGroupName(string s)
        {
            var res = _quickSearchService.SearchUserGroup(s);
            return this.ApiResp(res);
        }
        public IActionResult FileItem(string s)
        {
            var res = _quickSearchService.SearchFileItem(s);
            return this.ApiResp(res);
        }
        public IActionResult Material(string s)
        {
            var res = _quickSearchService.SearchMaterial(s);
            return this.ApiResp(res);
        }
    }
}
