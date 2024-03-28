using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.Entities.Wiki;
using FCloud3.Entities.Identities;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Wiki
{
    public class WikiItemController : Controller, IAuthGrantTypeProvidedController
    {
        private readonly WikiItemService _wikiService;
        private readonly HttpUserInfoService _userInfo;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.WikiItem;

        public WikiItemController(WikiItemService wikiService, HttpUserInfoService userInfo)
        {
            _wikiService = wikiService;
            _userInfo = userInfo;
        }

        [Authorize]
        [AuthGranted(AuthGrantOn.Dir, nameof(dirId))]
        public IActionResult CreateInDir(string title,string urlPathName,int dirId)
        {
            if(!_wikiService.CreateInDir(title,urlPathName,dirId,out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted(AuthGrantOn.Dir, nameof(dirId))]
        public IActionResult RemoveFromDir(int wikiId, int dirId)
        {
            if (!_wikiService.RemoveFromDir(wikiId, dirId, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        public IActionResult Edit(string urlPathName)
        {
            WikiItem? w = _wikiService.GetInfo(urlPathName, out string? errmsg);
            if (w is null || errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new WikiItemComModel()
            {
                Id = w.Id,
                Title = w.Title,
                UrlPathName = w.UrlPathName,
            });
        }
        [Authorize]
        [AuthGranted]
        public IActionResult EditExe([FromBody] WikiItemComModel model)
        {
            if (!_wikiService.EditInfo(_userInfo.Id, model.Title, model.UrlPathName, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted]
        public IActionResult LoadSimple(int id)
        {
            var res = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(res);
        }
        [Authorize]
        [AuthGranted(nameof(id))]
        public IActionResult InsertPara(int id, int afterOrder, WikiParaType type)
        {
            var res = _wikiService.InsertPara(id, afterOrder, type, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            var newList = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(newList);
        }
        [Authorize]
        [AuthGranted]
        public IActionResult SetParaOrders([FromBody]WikiItemParaOrdersComModel model)
        {
            var orderedParaIds = model.OrderedParaIds ?? throw new Exception("Ids参数为空");
            var res = _wikiService.SetParaOrders(model.Id, orderedParaIds, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            var newList = _wikiService.GetWikiParaDisplays(model.Id);
            return this.ApiResp(newList);
        }
        [Authorize]
        [AuthGranted(nameof(id))]
        public IActionResult RemovePara(int id,int paraId)
        {
            if (id == 0 || paraId == 0)
                return BadRequest();
            if (!_wikiService.RemovePara(id, paraId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            var newList = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(newList);
        }

        public IActionResult Index([FromBody]IndexRequestModel query)
        {
            var res = (_wikiService.Index(query));
            return this.ApiResp(res);
        }

        public class WikiItemComModel : IAuthGrantableRequestModel
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? UrlPathName { get; set; }
            public int AuthGrantOnId => Id;
        }
        public class WikiItemParaOrdersComModel : IAuthGrantableRequestModel
        {
            public int Id { get; set; }
            public List<int>? OrderedParaIds { get; set; }
            public int AuthGrantOnId => Id;
        }
    }
}
