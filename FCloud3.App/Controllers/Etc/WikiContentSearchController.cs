using FCloud3.App.Services.Utils;
using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class WikiContentSearchController(
        WikiContentSearchService wikiContentSearchService,
        HttpUserInfoService httpUserInfoService
        ):Controller
    {
        private readonly WikiContentSearchService _wikiContentSearchService = wikiContentSearchService;
        private readonly HttpUserInfoService _httpUserInfoService = httpUserInfoService;
        public IActionResult Search(string str)
        {
            var isAdmin = _httpUserInfoService.IsAdmin;
            var res = _wikiContentSearchService.Search(str, isAdmin);
            return this.ApiResp(res);
        }
    }
}