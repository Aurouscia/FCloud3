using System.Net;
using FCloud3.App.Models.COM;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FCloud3.App.Services.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RateLimitedAttribute: Attribute, IFilterFactory
    {
        private readonly int slidingWindowMs;
        private readonly int maxCountWithin;
        public RateLimitedAttribute(int slidingWindowMs = 3000, int maxCountWithin = 1)
        {
            this.slidingWindowMs = slidingWindowMs;
            this.maxCountWithin = maxCountWithin;
        }
        
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var f = serviceProvider.GetRequiredService<RateLimitedActionFilter>();
            f.MaxCountWithin = this.maxCountWithin;
            f.SlidingWindowMs = this.slidingWindowMs;
            return f;
        }

        public bool IsReusable => false;
    }

    public class RateLimitedActionFilter : IAsyncActionFilter
    {
        public int SlidingWindowMs { get; set; }
        public int MaxCountWithin { get; set; }
        private readonly static Dictionary<IPAddress, Dictionary<string, List<DateTime>>> records = [];
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ip = context.HttpContext.Connection.RemoteIpAddress;
            if (ip is null)
            {
                var resp = new ApiResponse(null, false, "获取IP失败");
                context.Result = resp.BuildResult();
                return;
            }
            var actionName = context.ActionDescriptor.DisplayName ?? "";
            DateTime now = DateTime.Now;

            if (!records.TryGetValue(ip, out var actionRecords))
            {
                actionRecords = [];
                records.Add(ip, actionRecords);
            }
            if (!actionRecords.TryGetValue(actionName, out var itsRecords))
            {
                itsRecords = [];
                actionRecords.Add(actionName, itsRecords);
            }
            itsRecords.Add(now);
            itsRecords.RemoveAll(x => (now - x).TotalMilliseconds > SlidingWindowMs);
            if (itsRecords.Count > MaxCountWithin)
            {
                var resp = new ApiResponse(null, false, 
                    $"请求频率过高，请注意{SlidingWindowMs}毫秒内最多{MaxCountWithin}个请求");
                //HTTP 429 TooManyRequests
                context.Result = resp.BuildResult(429);
                return;
            }
            await next();
        }
    }
    
    public static class RateLimitedAttributeExtension
    {
        public static IServiceCollection AddRateLimitedFilter(this IServiceCollection services)
        {
            services.AddScoped<RateLimitedActionFilter>();
            return services;
        }
    }
}