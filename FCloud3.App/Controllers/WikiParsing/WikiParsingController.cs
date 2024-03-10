using FCloud3.Services.WikiParsing;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.WikiParsing
{
    public class WikiParsingController : Controller
    {
        private readonly WikiParsingService _wikiParsingService;

        public WikiParsingController(WikiParsingService wikiParsingService) 
        {
            _wikiParsingService = wikiParsingService;
        }

        public IActionResult GetParsedWiki(string pathName)
        {
            var res =  _wikiParsingService.GetParsedWiki(pathName);
            return this.ApiResp(res);
        }
    }
}
