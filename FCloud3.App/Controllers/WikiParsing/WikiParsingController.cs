using FCloud3.Services.WikiParsing;
using FCloud3.Services.WikiParsing.Support;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.App.Controllers.WikiParsing
{
    public class WikiParsingController : Controller
    {
        private readonly WikiParsingService _wikiParsingService;
        private readonly WikiParsingRulesProviderService _wikiParsingRulesProvider;

        public WikiParsingController(
            WikiParsingService wikiParsingService,
            WikiParsingRulesProviderService wikiParsingRulesProvider) 
        {
            _wikiParsingService = wikiParsingService;
            _wikiParsingRulesProvider = wikiParsingRulesProvider;
        }

        public IActionResult GetParsedWiki(string pathName)
        {
            var res =  _wikiParsingService.GetParsedWikiStream(pathName);
            if(res is not null)
                return File(res, Application.Json);
            return this.ApiFailedResp("找不到该词条");
        }

        public IActionResult GetRulesCommons([FromBody] List<string> ruleNames)
        {
            var res = _wikiParsingRulesProvider.GetCommonsOfRules(ruleNames);
            return this.ApiResp(res);
        }
    }
}
