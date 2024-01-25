using FCloud3.Services.Sys;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Sys
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
    }
}
