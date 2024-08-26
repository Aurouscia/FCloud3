using Microsoft.Extensions.Logging;

namespace FCloud3.App.Services.Middlewares
{
    public class RequestAnomalyRecorder
    {
        private readonly RequestDelegate _next;
        public RequestAnomalyRecorder(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ILogger<RequestAnomalyRecorder> logger)
        {
            CancellationTokenSource cts = new();
            var token = cts.Token;
            var record = Task.Run(async() =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                if (!token.IsCancellationRequested)
                {
                    var path = context.Request.Path.ToString();
                    var query = context.Request.QueryString.ToString();
                    logger.LogCritical("[长时间无响应]{path} {query}", path, query);
                }
            });
            await _next(context);
            cts.Cancel();
        }
    }
}