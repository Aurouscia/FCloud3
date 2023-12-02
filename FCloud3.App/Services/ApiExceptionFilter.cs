using FCloud3.App.Models.COM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.App.Services
{
    public class ApiExceptionFilter:ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }
        public override void OnException(ExceptionContext context)
        {
            // 如果异常没有被处理则进行处理
            if (context.ExceptionHandled == false)
            {
                Exception ex = context.Exception;
                string msg = ex.Message ?? "服务器内部错误";
                string path = context.HttpContext.Request.Path;
                string method = context.HttpContext.Request.Method;

                _logger.LogError(ex, "未经处理的异常 {Method} {path}",method,path);

                context.Result = new ContentResult()
                {
                    StatusCode = 200,
                    Content = JsonConvert.SerializeObject(new ApiResponse(null, false, msg)),
                    ContentType = Application.Json
                };
            }
            // 设置为true，表示异常已经被处理了
            context.ExceptionHandled = true;
        }
    }
}
