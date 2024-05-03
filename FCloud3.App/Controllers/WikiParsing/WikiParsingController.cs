using FCloud3.Services.Wiki.Support;
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
        private readonly WikiRecommendService _wikiRecommendService;

        public WikiParsingController(
            WikiParsingService wikiParsingService,
            WikiParsingRulesProviderService wikiParsingRulesProvider,
            WikiRecommendService wikiRecommendService) 
        {
            _wikiParsingService = wikiParsingService;
            _wikiParsingRulesProvider = wikiParsingRulesProvider;
            _wikiRecommendService = wikiRecommendService;
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

        public IActionResult GetRecommends(string pathName)
        {
            return this.ApiResp(_wikiRecommendService.Get(pathName));
        }
    }
}
