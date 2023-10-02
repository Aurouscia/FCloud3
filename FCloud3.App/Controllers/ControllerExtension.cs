using FCloud3.App.Models.COM;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FCloud3.App.Controllers
{
    public static class ControllerExtension
    {
        private const string jsonContentType = "application/json";
        public static ContentResult ApiResp(this Controller controller, object? obj=null, bool success = true)
        {
            ContentResult result = new()
            {
                StatusCode = 200,
                Content = JsonConvert.SerializeObject(new ApiResponse(obj, success)),
                ContentType = jsonContentType
            };
            return result;
        }
        public static ContentResult ApiFailedResp(this Controller controller, string? errmsg)
        {
            ContentResult result = new()
            {
                StatusCode = 200,
                Content = JsonConvert.SerializeObject(new ApiResponse(null, false, errmsg)),
                ContentType = jsonContentType
            };
            return result;
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
