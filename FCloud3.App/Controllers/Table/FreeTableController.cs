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
            var res = _freeTableService.GetById(id);
            if (res is null)
                return this.ApiFailedResp("未找到指定的表格");
            return this.ApiResp(res);
        }

        [AuthGranted]
        [UserActiveOperation]
        [UserTypeRestricted]
        public IActionResult SaveInfo(int id, string title)
        {
            if (!_freeTableService.TryEditInfo(id, title, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [AuthGranted]
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
