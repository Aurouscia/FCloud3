using FCloud3.App.Services;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Wiki
{
    public class WikiItemController:Controller
    {
        private readonly WikiItemService _wikiService;
        private readonly HttpUserInfoService _userInfo;

        public WikiItemController(WikiItemService wikiService,HttpUserInfoService userInfo)
        {
            _wikiService = wikiService;
            _userInfo = userInfo;
        }

        [Authorize]
        public IActionResult Create(string title)
        {
            if(!_wikiService.TryAdd(_userInfo.Id,title,out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        public IActionResult Edit(int id)
        {
            WikiItem w = _wikiService.GetById(id)??throw new Exception("找不到指定id的wiki");
            return this.ApiResp(new WikiItemComModel()
            {
                Id = w.Id,
                Title = w.Title,
            });
        }
        [Authorize]
        public IActionResult EditExe([FromBody]WikiItemComModel model)
        {
            if (!_wikiService.TryEdit(_userInfo.Id, model.Title, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult LoadSimple(int id)
        {
            var res = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(res);
        }
        public IActionResult InsertPara(int id, int afterOrder, WikiParaType type)
        {
            var res = _wikiService.InsertPara(id, afterOrder, type, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            var newList = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(newList);
        }
        public IActionResult SetParaOrders([FromBody]WikiItemParaOrdersComModel model)
        {
            var orderedParaIds = model.OrderedParaIds ?? throw new Exception("Ids参数为空");
            var res = _wikiService.SetParaOrders(model.Id, orderedParaIds, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            var newList = _wikiService.GetWikiParaDisplays(model.Id);
            return this.ApiResp(newList);
        }
        public IActionResult RemovePara(int id,int paraId)
        {
            if (id == 0 || paraId == 0)
                return BadRequest();
            if (!_wikiService.TryRemovePara(id, paraId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            var newList = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(newList);
        }

        public class WikiItemComModel
        {
            public int Id { get; set; }
            public string? Title { get; set; }
        }
        public class WikiItemParaOrdersComModel
        {
            public int Id { get; set; }
            public List<int>? OrderedParaIds { get; set; }
        }
    }
}
