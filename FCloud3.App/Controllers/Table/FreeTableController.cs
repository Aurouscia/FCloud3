using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Services.Table;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace FCloud3.App.Controllers.Table
{
    [Authorize]
    public class FreeTableController:Controller, IAuthGrantTypeProvidedController
    {
        private readonly FreeTableService _freeTableService;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.FreeTable;

        public FreeTableController(FreeTableService freeTableService) 
        {
            _freeTableService = freeTableService;
        }

        [AuthGranted(AuthGrantOn.WikiPara)]
        public IActionResult CreateForPara(int paraId)
        {
            int createdId = _freeTableService.TryAddAndAttach(paraId, out string? errmsg);
            if (createdId <= 0)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new { CreatedId = createdId });
        }

        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult Load(int id)
        {
            var res = _freeTableService.GetForEditing(id, out string? errmsg);
            if(res is null || errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }

        public IActionResult GetMeta(int id)
        {
            var res = _freeTableService.GetMeta(id);
            if (res is not null)
                return this.ApiResp(res);
            return this.ApiFailedResp("找不到指定表格");
        }

        [AuthGranted(nameof(id))]
        [UserActiveOperation]
        [UserTypeRestricted]
        public IActionResult SaveInfo(int id, string title)
        {
            if (!_freeTableService.TryEditInfo(id, title, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [AuthGranted(nameof(id))]
        [UserActiveOperation]
        [UserTypeRestricted]
        public IActionResult SaveContent(int id, string data)
        {
            if (!_freeTableService.TryEditContent(id, data, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}
