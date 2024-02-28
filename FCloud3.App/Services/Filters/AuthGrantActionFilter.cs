using FCloud3.App.Controllers.Files;
using FCloud3.App.Controllers.Wiki;
using FCloud3.App.Models.COM;
using FCloud3.Entities.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FCloud3.App.Services.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthGrantedAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<AuthGrantActionFilter>();
        }

        public class AuthGrantActionFilter : IAsyncActionFilter
        {
            private readonly AuthGrantService _authGrantService;
            public AuthGrantActionFilter(AuthGrantService authGrantService)
            {
                _authGrantService = authGrantService;
            }
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var model = context.ActionArguments.Values.FirstOrDefault(x => x is IAuthGrantableRequestModel) as IAuthGrantableRequestModel
                    ?? throw new Exception("无法验证提交数据");
                int id = model.AuthGrantOnId;
                var controller = context.Controller;

                AuthGrantOn on = AuthGrantOn.None;
                if (controller is FileDirController)
                    on = AuthGrantOn.Dir;
                else if (controller is WikiItemController)
                    on = AuthGrantOn.Wiki;
                if (on == AuthGrantOn.None)
                    throw new Exception("权限验证未找到匹配类型");

                if (!_authGrantService.Test(on, id))
                {
                    var resp = new ApiResponse(null, false, "无权限");
                    context.Result = resp.BuildResult();
                }
                else
                    await next();
            }
        }
    }

    public static class AuthGrantedAttributeService
    {
        public static IServiceCollection AddAuthGrantedActionFilter(this IServiceCollection services)
        {
            services.AddScoped<AuthGrantedAttribute.AuthGrantActionFilter>();
            return services;
        }
    }
}
