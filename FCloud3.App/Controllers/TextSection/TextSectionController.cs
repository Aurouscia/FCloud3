using FCloud3.Repos.TextSec;
using FCloud3.Services.TextSec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Mechanics;
using FCloud3.Entities.TextSection;
using FCloud3.App.Services.Utils;

namespace FCloud3.App.Controllers.TextSec
{
    public class TextSectionController:Controller
    {
        private readonly TextSectionService _textSectionService;
        private readonly HtmlGenParserProvider _genParser;

        public TextSectionController(
            TextSectionService textSectionService,
            HtmlGenParserProvider genParser) 
        {
            _textSectionService = textSectionService;
            _genParser = genParser;
        }

        public IActionResult CreateForPara(int paraId)
        {
            int createdId = _textSectionService.TryAddAndAttach(paraId, out string? errmsg);
            if (createdId <= 0)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new { CreatedId = createdId });
        }

        public IActionResult Edit(int id)
        {
            var res = _textSectionService.GetById(id) ?? throw new Exception("找不到指定Id的文本段");
            TextSectionComModel model = new(res);
            return this.ApiResp(model);
        }
        [Authorize]
        public IActionResult EditExe([FromBody] TextSectionComModel model)
        {
            if (!_textSectionService.TryUpdate(model.Id, model.Title, model.Content, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        [Authorize]
        public IActionResult Preview(int id, string content)
        {
            var parser = _genParser.GetParser(id);
            var res = new TextSectionPreviewResponse(parser.RunToStructured(content));
            return this.ApiResp(res);
        }


        public class TextSectionComModel
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Content { get; set; }

            public TextSectionComModel() { }
            public TextSectionComModel(TextSection original)
            {
                Id = original.Id;
                Title = original.Title;
                Content = original.Content;
            }
        }
        public class TextSectionPreviewResponse
        {
            public string HtmlSource { get; }
            public string PreScripts { get; }
            public string PostScripts { get; }
            public string Styles { get; }
            public TextSectionPreviewResponse(string htmlSource)
            {
                HtmlSource = htmlSource;
                PreScripts = "";
                PostScripts = "";
                Styles = "";
            }
            public TextSectionPreviewResponse(ParserResult parserResult)
            {
                HtmlSource = parserResult.Content + parserResult.FootNotes;
                PreScripts = parserResult.PreScript;
                PostScripts = parserResult.PostScript;
                Styles = parserResult.Style;
            }
        }
    }
}
