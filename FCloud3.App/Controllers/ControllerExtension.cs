using FCloud3.App.Models.COM;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FCloud3.App.Controllers
{
    public static class ControllerExtension
    {
        public static ContentResult ApiResp(this Controller _, object? obj=null, bool success = true)
        {
            var resp = new ApiResponse(obj, success);
            return resp.BuildResult();
        }
        public static ContentResult ApiFailedResp(this Controller _, string? errmsg)
        {
            var resp = new ApiResponse(null, false, errmsg);
            return resp.BuildResult();
        }
        public static ContentResult ApiResp(this Controller controller, string? errmsg)
        {
            if (errmsg is null)
                return controller.ApiResp();
            else
                return controller.ApiFailedResp(errmsg);
        }
    }
}
