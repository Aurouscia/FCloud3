using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using FCloud3.Services.TextSec;
using FCloud3.Services.Wiki;
using FCloud3.Services.WikiParsing.Support;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.TextSection
{
    [Authorize]
    public class TextSectionController:Controller, IAuthGrantTypeProvidedController
    {
        private readonly TextSectionService _textSectionService;
        private readonly WikiParserProviderService _genParser;
        private readonly WikiTitleContainService _titleContainService;
        private readonly WikiParaService _wikiParaService;
        private readonly ILocatorHash _locatorHash;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.TextSection;

        public TextSectionController(
            TextSectionService textSectionService,
            WikiParserProviderService genParser,
            WikiTitleContainService titleContainService,
            WikiParaService wikiParaService,
            ILocatorHash locatorHash) 
        {
            _textSectionService = textSectionService;
            _genParser = genParser;
            _titleContainService = titleContainService;
            _wikiParaService = wikiParaService;
            _locatorHash = locatorHash;
        }

        [AuthGranted(AuthGrantOn.WikiPara)]
        public IActionResult CreateForPara(int paraId)
        {
            int createdId = _textSectionService.TryAddAndAttach(paraId, out string? errmsg);
            if (createdId <= 0)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new { CreatedId = createdId });
        }

        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult Edit(int id)
        {
            var res = _textSectionService.GetForEditing(id, out string? errmsg);
            if(errmsg is not null || res is null)
                return this.ApiFailedResp(errmsg);
            TextSectionComModel model = new(res);
            return this.ApiResp(model);
        }
        
        /// <summary>
        /// 更新文本段，可只更新标题或只更新内容，另一者设为null即可
        /// </summary>
        /// <param name="model">模型</param>
        /// <returns></returns>
        [AuthGranted]
        [UserTypeRestricted]
        [UserActiveOperation]
        public IActionResult EditExe([FromBody] TextSectionComModel model)
        {
            if (!_textSectionService.TryUpdate(model.Id, model.Title, model.Content, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult GetMeta(int id)
        {
            var res = _textSectionService.GetMeta(id);
            if (res is not null)
                return this.ApiResp(res);
            return this.ApiFailedResp("找不到指定文本段");
        }

        [AuthGranted(nameof(id))]
        [UserTypeRestricted]
        public IActionResult Preview(int id, string content)
        {
            var res = _textSectionService.Preview(id, content);
            return this.ApiResp(res);
        }


        public class TextSectionComModel:IAuthGrantableRequestModel
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Content { get; set; }
            public int AuthGrantOnId => Id;

            public TextSectionComModel() { }
            public TextSectionComModel(Entities.TextSection.TextSection original)
            {
                Id = original.Id;
                Title = original.Title;
                Content = original.Content;
            }
        }
    }
}
