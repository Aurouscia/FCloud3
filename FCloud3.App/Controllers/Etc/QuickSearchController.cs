using FCloud3.App.Services.Utils;
using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class QuickSearchController(
        QuickSearchService quickSearchService,
        HttpUserInfoService httpUserInfoService)
        : Controller
    {
        private readonly QuickSearchService 
            _quickSearchService = quickSearchService;
        private readonly HttpUserInfoService
            _httpUserInfoService = httpUserInfoService;

        public IActionResult WikiItem(string s, int excludeDir)
        {
            var isAdmin = _httpUserInfoService.IsAdmin;
            var res = _quickSearchService.SearchWikiItem(s, excludeDir, isAdmin);
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
        public IActionResult FileDir(string s)
        {
            var res = _quickSearchService.SearchFileDir(s);
            return this.ApiResp(res);
        }
    }
}
