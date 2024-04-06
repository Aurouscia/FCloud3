using FCloud3.App.Models.COM;
using FCloud3.App.Services.Utils;
using FCloud3.Entities.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FCloud3.App.Services.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UserTypeRestrictedAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;
        public UserType Level { get; set; }
        public UserTypeRestrictedAttribute(UserType level = UserType.Member)
        {
            Level = level;
        }
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var f = serviceProvider.GetRequiredService<UserTypeRestrictedFilter>();
            f.Level = this.Level;
            return f;
        }

        public class UserTypeRestrictedFilter : IAsyncResourceFilter
        {
            private readonly HttpUserIdProvider _httpUserIdProvider;
            private readonly UserService _userService;

            public UserType Level { get; set; }
            public UserTypeRestrictedFilter(
                HttpUserIdProvider httpUserIdProvider,
                UserService userService)
            {
                _httpUserIdProvider = httpUserIdProvider;
                _userService = userService;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                int userId = _httpUserIdProvider.Get();
                var type = _userService.GetUserType(userId);
                if(type < Level)
                {
                    int code = Level == UserType.Member ? ApiResponseCodes.NoTourist : 0;
                    var resp = new ApiResponse(null, false, $"需要<{_userService.UserTypeText(Level)}>身份", code);
                    context.Result = resp.BuildResult();
                }
                else
                    await next();
            }
        }
    }
    public static class UserTypeRestrictedAttributeExtensions
    {
        public static IServiceCollection AddUserTypeRestrictedAttribute(this IServiceCollection services)
        {
            services.AddScoped<UserTypeRestrictedAttribute.UserTypeRestrictedFilter>();
            return services;
        }
    }
}
