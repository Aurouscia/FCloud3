using FCloud3.App.Models.COM.TextSec;
using FCloud3.App.Services;
using FCloud3.Repos.Models.Cor;
using FCloud3.Services.TextSec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.TextSec
{
    public class TextSectionController:Controller
    {
        private readonly TextSectionService _textSectionService;
        private readonly HttpUserInfoService _user;

        public TextSectionController(HttpUserInfoService user, TextSectionService textSectionService) 
        {
            _user = user;
            _textSectionService = textSectionService;
        }

        public IActionResult CreateForCorr(int corrId)
        {
            int createdId = _textSectionService.TryAddAndAttach(_user.Id, corrId, out string? errmsg);
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
            if (!_textSectionService.TryUpdate(model.Id, _user.Id, model.Title, model.Content, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}
