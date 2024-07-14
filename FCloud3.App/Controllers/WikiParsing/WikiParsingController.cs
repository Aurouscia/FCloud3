using FCloud3.App.Services.Utils;
using FCloud3.Entities.Identities;
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
        private readonly HttpUserInfoService _httpUserInfoService;

        public WikiParsingController(
            WikiParsingService wikiParsingService,
            WikiParsingRulesProviderService wikiParsingRulesProvider,
            WikiRecommendService wikiRecommendService,
            HttpUserInfoService httpUserInfoService) 
        {
            _wikiParsingService = wikiParsingService;
            _wikiParsingRulesProvider = wikiParsingRulesProvider;
            _wikiRecommendService = wikiRecommendService;
            _httpUserInfoService = httpUserInfoService;
        }

        public IActionResult GetParsedWiki(string pathName)
        {
            bool isAdmin = _httpUserInfoService.IsAdmin;
            var res =  _wikiParsingService.GetParsedWikiStream(pathName, isAdmin);
            if(res is not null)
                return File(res, Application.Json);
            return this.ApiFailedResp("找不到该词条");
        }

        public IActionResult GetWikiDisplayInfo(string pathName)
        {
            bool isAdmin = _httpUserInfoService.IsAdmin;
            var res = _wikiParsingService.GetWikiDisplayInfo(pathName, defaultAccess: isAdmin);
            if (res is null)
                return this.ApiFailedResp("找不到该词条");
            return this.ApiResp(res);
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
