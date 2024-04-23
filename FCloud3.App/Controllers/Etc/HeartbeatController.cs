using FCloud3.Services.Etc.TempData.EditLock;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class HeartbeatController: Controller
    {
        private readonly ContentEditLockService _contentEditLockService;

        public HeartbeatController(ContentEditLockService contentEditLockService)
        {
            _contentEditLockService = contentEditLockService;
        }

        public IActionResult Do(ObjectType objType, int objId)
        {
            if (!_contentEditLockService.Heartbeat(objType, objId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}
