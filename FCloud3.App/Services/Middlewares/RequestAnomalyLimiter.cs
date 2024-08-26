using System.Net;
using FCloud3.App.Models.COM;

namespace FCloud3.App.Services.Middlewares
{
    public class RequestAnomalyLimiter
    {
        private readonly RequestDelegate _next;
        public RequestAnomalyLimiter(RequestDelegate next)
        {
            _next = next;
        }

        private const int maxProcessing = 5;
        private static Dictionary<IPAddress, int> record = [];
        private static object lockObj = new();
        private static int Incre(IPAddress ip, int by)
        {
            lock (lockObj)
            {
                record.TryGetValue(ip, out int processing);
                if (by == 0)
                    return processing;
                processing += by;
                record[ip] = processing;
                return processing;
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress;
            if (ip is not null)
            {
                var processing = Incre(ip, 0);
                if (processing >= maxProcessing)
                {
                    var resp = new ApiResponse(null, false, "服务阻塞，请联系管理员");
                    await context.Response.WriteAsJsonAsync(resp);
                    return;
                }
                Incre(ip, 1);
            }
            await _next(context);
            if (ip is not null)
            {
                Incre(ip, -1);
            }
        }
    }
}