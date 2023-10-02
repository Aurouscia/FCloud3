using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FCloud3.Utils.Utils.Logger;

namespace FCloud3.App.Controllers
{
    public class ErrorController : Controller
    {
        private readonly FLogger _logger;

        public ErrorController(FLogger logger)
        {
            _logger = logger;
        }
        [Route("/Error")]
        public IActionResult Index()
        {
            IExceptionHandlerPathFeature? iExceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            string? err = "服务器内部错误";
            if (iExceptionHandlerFeature != null)
            {
                string path = iExceptionHandlerFeature.Path;
                Exception exception = iExceptionHandlerFeature.Error;
                _logger.LogError("未处理的异常", exception.Message);
                err = exception.Message;
            }
            return this.ApiFailedResp(err);
        }
    }
}