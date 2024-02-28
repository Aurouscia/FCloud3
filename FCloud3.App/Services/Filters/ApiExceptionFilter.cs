using FCloud3.App.Models.COM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.App.Services.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
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

                _logger.LogError(ex, "未经处理的异常 {Method} {path}", method, path);

                ApiResponse resp = new(null, false, "未知错误:" + msg);
                context.Result = resp.BuildResult();
            }
            // 设置为true，表示异常已经被处理了
            context.ExceptionHandled = true;
        }
    }
}
