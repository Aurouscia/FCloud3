using FCloud3.App.Services.Filters;
using FCloud3.Services.Table;
using Microsoft.AspNetCore.Mvc;


namespace FCloud3.App.Controllers.Table
{
    public class FreeTableController:Controller
    {
        private readonly FreeTableService _freeTableService;

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

        public IActionResult Load(int id)
        {
            var res = _freeTableService.GetById(id);
            if (res is null)
                return this.ApiFailedResp("未找到指定的表格");
            return this.ApiResp(res);
        }

        public IActionResult SaveInfo(int id, string title)
        {
            if (!_freeTableService.TryEditInfo(id, title, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [UserActiveOperation]
        public IActionResult SaveContent(int id, string data)
        {
            if (!_freeTableService.TryEditContent(id, data, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}
