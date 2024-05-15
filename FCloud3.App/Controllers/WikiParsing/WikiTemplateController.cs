using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Entities.WikiParsing;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.Services.WikiParsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.WikiParsing
{
    [Authorize]
    public class WikiTemplateController : Controller
    {
        private readonly WikiTemplateService _wikiTemplateService;
        private readonly Services.Utils.WikiParserProviderService _wikiParserProviderService;

        public WikiTemplateController(
            WikiTemplateService wikiTemplateService,
            Services.Utils.WikiParserProviderService wikiParserProviderService) 
        {
            _wikiTemplateService = wikiTemplateService;
            _wikiParserProviderService = wikiParserProviderService;
        }
        public IActionResult GetList(string? search)
        {
            var res = _wikiTemplateService.GetList(search ?? "");
            return this.ApiResp(res);
        }
        [UserTypeRestricted(UserType.SuperAdmin)]
        public IActionResult Add(string name)
        {
            var res = _wikiTemplateService.Add(name, out var errmsg);
            if (!res || !string.IsNullOrEmpty(errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [UserTypeRestricted(UserType.SuperAdmin)]
        public IActionResult Edit(int id)
        {
            var res = _wikiTemplateService.Edit(id);
            return this.ApiResp(res);
        }
        [UserTypeRestricted(UserType.SuperAdmin)]
        public IActionResult EditExe([FromBody]WikiTemplate wikiTemplate)
        {
            var res = _wikiTemplateService.EditExe(wikiTemplate, out string? errmsg);
            if (!res || !string.IsNullOrEmpty(errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [UserTypeRestricted(UserType.SuperAdmin)]
        public IActionResult Remove(int id)
        {
            var res = _wikiTemplateService.Remove(id, out string? errmsg);
            if(!res || !string.IsNullOrEmpty(errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [UserTypeRestricted(UserType.SuperAdmin)]
        public IActionResult Preview([FromBody]WikiTemplate wikiTemplate)
        {
            if (string.IsNullOrWhiteSpace(wikiTemplate.Name))
                return this.ApiFailedResp("名称不能为空");
            if (string.IsNullOrEmpty(wikiTemplate.Source))
                return this.ApiFailedResp("html不能为空");

            string cacheKey = $"tplte_{wikiTemplate.Id}";
            var parser = _wikiParserProviderService.GetParser(cacheKey, 
                x=>x.Cache.DisableCache());

            Template t = new(wikiTemplate.Name, wikiTemplate.Source, wikiTemplate.Styles, wikiTemplate.PreScripts, wikiTemplate.PostScripts);
            parser.Context.Options.TemplateParsingOptions.ClearTemplates();
            parser.Context.Options.TemplateParsingOptions.AddTemplates(new() { t });
            parser.Context.TemplateSlotInfo.Clear();

            var res = parser.RunToParserResult(wikiTemplate.Demo);
            return this.ApiResp(new WikiTemplatePreviewResponse(res));
        }


        public class WikiTemplatePreviewResponse
        {
            public string HtmlSource { get; }
            public string PreScripts { get; }
            public string PostScripts { get; }
            public string Styles { get; }
            public WikiTemplatePreviewResponse(ParserResult parserResult)
            {
                HtmlSource = parserResult.Content + parserResult.FootNotes;
                PreScripts = parserResult.PreScript;
                PostScripts = parserResult.PostScript;
                Styles = parserResult.Style;
            }
        }
    }
}
