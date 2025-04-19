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

        public IActionResult GetInfoById(int id)
        {
            var info = _wikiService.GetInfoById(id);
            if (info is null)
                return this.ApiFailedResp("找不到指定词条");
            return this.ApiResp(new WikiItemComModel()
            {
                Id = info.Id,
                UrlPathName = info.UrlPathName,
                Title = info.Title,
                OwnerId = info.OwnerId
            });
        }

        public IActionResult ViewDirLocations(string urlPathName)
        {
            var model = _wikiService.ViewDirLocations(urlPathName);
            return this.ApiResp(model);
        }

        [Authorize]
        [AuthGranted(AuthGrantOn.Dir, nameof(dirId))]
        [UserTypeRestricted]
        public IActionResult CreateInDir(string title,string urlPathName,int dirId)
        {
            if(!_wikiService.CreateInDir(title,urlPathName,dirId,out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        [Authorize]
        [UserTypeRestricted]
        public IActionResult Create(string title,string urlPathName)
        {
            if (!_wikiService.Create(title, urlPathName, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted(AuthGrantOn.Dir, nameof(dirId))]
        [UserTypeRestricted]
        public IActionResult RemoveFromDir(int wikiId, int dirId)
        {
            if (!_wikiService.RemoveFromDir(wikiId, dirId, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult Delete(int id)
        {
            var isSuperAdmin = _userInfo.IsSuperAdmin;
            if (!_wikiService.Delete(id, isSuperAdmin, out string? errmsg))
                return this.ApiFailedResp(errmsg);
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
                OwnerId = w.OwnerUserId
            });
        }
        [Authorize]
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult EditExe([FromBody] WikiItemComModel model)
        {
            //并不使用model的ownerId属性，无overposting风险
            if (!_wikiService.EditInfo(model.Id, model.Title, model.UrlPathName, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        [Authorize]
        public IActionResult Transfer(int wikiId, int uid)
        {
            var isSuperAdmin = _userInfo.IsSuperAdmin;
            if (!_wikiService.Transfer(wikiId, uid, isSuperAdmin, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult LoadSimple(int id)
        {
            var res = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(res);
        }
        [Authorize]
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult LoadFull(int id)
        {
            var res = _wikiService.GetWikiParaContents(id, out string? errmsg);
            if(res is not null)
                return this.ApiResp(res);
            return this.ApiFailedResp(errmsg);
        }
        [Authorize]
        [AuthGranted(nameof(id))]
        [UserTypeRestricted]
        public IActionResult InsertPara(int id, int afterOrder, int copySrc, WikiParaType type)
        {
            var newlyCreatedParaId = _wikiService.InsertPara(id, afterOrder, type, copySrc, out string? errmsg);
            if (newlyCreatedParaId <= 0)
                return this.ApiFailedResp(errmsg);
            var newList = _wikiService.GetWikiParaDisplays(id);
            return this.ApiResp(newList);
        }
        [Authorize]
        [AuthGranted(nameof(id))]
        [UserTypeRestricted]
        public IActionResult InsertParaAndGetId(int id, int afterOrder, WikiParaType type)
        {
            var newlyCreatedParaId = _wikiService.InsertPara(id, afterOrder, type, 0, out string? errmsg);
            if (newlyCreatedParaId <= 0)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new { newlyCreatedParaId });
        }
        
        [Authorize]
        [AuthGranted]
        [UserTypeRestricted]
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
        [UserTypeRestricted]
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

        [Authorize]
        [UserTypeRestricted(UserType.Admin)]
        public IActionResult SetSealed(int id, bool @sealed)
        {
            var res = _wikiService.SetSealed(id, @sealed, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class WikiItemComModel : IAuthGrantableRequestModel
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? UrlPathName { get; set; }
            public int OwnerId { get; set; }
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
