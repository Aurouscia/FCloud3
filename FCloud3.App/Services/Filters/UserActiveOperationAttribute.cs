using FCloud3.App.Services.Utils;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.App.Services.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UserActiveOperationAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public UserActiveOperationAttribute()
        {
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var f = serviceProvider.GetRequiredService<UserActiveActionFilter>();
            return f;
        }

        public class UserActiveActionFilter : IAsyncActionFilter
        {
            private readonly UserService _userService;
            private readonly IMemoryCache _cache;
            private readonly HttpUserIdProvider _userIdProvider;

            public UserActiveActionFilter(UserService userService, IMemoryCache cache, HttpUserIdProvider userIdProvider)
            {
                _userService = userService;
                _cache = cache;
                _userIdProvider = userIdProvider;
            }
            private const int cacheExpireMinutes = 30;
            private const int updateThresholdMinutes = 3;
            private static string CacheKey(int uid) => $"UserActiveTimeUpdated_{uid}";
            public DateTime LastUpdatedUserActive(int uid)
            {
                return _cache.Get<DateTime>(CacheKey(uid));
            }
            public void SetLastUpdatedUserActive(int uid)
            {
                _cache.Set(CacheKey(uid), DateTime.Now, TimeSpan.FromMinutes(cacheExpireMinutes));
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                int uid = _userIdProvider.Get();
                if (uid > 0)
                {
                    var time = LastUpdatedUserActive(uid);
                    if (DateTime.Now - time > TimeSpan.FromMinutes(updateThresholdMinutes))
                    {
                        _userService.SetLastUpdateToNow();
                        SetLastUpdatedUserActive(uid);
                    }
                }
                await next();
            }
        }
    }

    public static class UserActiveOperationAttributeExtensions
    {
        public static IServiceCollection AddUserActiveOperationFilter(this IServiceCollection services)
        {
            services.AddScoped<UserActiveOperationAttribute.UserActiveActionFilter>();
            return services;
        }
    }
}
