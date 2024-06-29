using FCloud3.Services.Etc;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class WikiContentSearchController(
        WikiContentSearchService wikiContentSearchService):Controller
    {
        private readonly WikiContentSearchService _wikiContentSearchService = wikiContentSearchService;
        public IActionResult Search(string str)
        {
            var res = _wikiContentSearchService.Search(str);
            return this.ApiResp(res);
        }
    }
}