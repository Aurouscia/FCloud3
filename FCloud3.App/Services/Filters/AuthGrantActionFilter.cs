using FCloud3.App.Controllers.Files;
using FCloud3.App.Controllers.Wiki;
using FCloud3.App.Models.COM;
using FCloud3.App.Services.Utils;
using FCloud3.Entities.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FCloud3.App.Services.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthGrantedAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;
        public AuthGrantOn OnType { get; }
        public string? FormKey { get; }
        public bool IgnoreZero { get; }

        public AuthGrantedAttribute(AuthGrantOn onType = AuthGrantOn.None, string? formKey = null, bool ignoreZero = false)
        {
            this.OnType = onType;
            this.FormKey = formKey;
            this.IgnoreZero = ignoreZero;
        }
        public AuthGrantedAttribute(bool ignoreZero)
        {
            IgnoreZero = ignoreZero;
        }
        public AuthGrantedAttribute(string formKey)
        {
            FormKey = formKey;
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var f = serviceProvider.GetRequiredService<AuthGrantActionFilter>();
            f.OnType = this.OnType;
            f.FormKey = this.FormKey;
            f.IgnoreZero = this.IgnoreZero;
            return f;
        }

        public class AuthGrantActionFilter : IAsyncActionFilter
        {
            private readonly AuthGrantService _authGrantService;
            private readonly HttpUserInfoService _httpUserInfoService;

            public AuthGrantOn OnType { get; set; }
            public string? FormKey { get; set; }
            public bool IgnoreZero { get; set; }
            public AuthGrantActionFilter(AuthGrantService authGrantService, HttpUserInfoService httpUserInfoService)
            {
                _authGrantService = authGrantService;
                _httpUserInfoService = httpUserInfoService;
            }
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (_httpUserInfoService.IsAdmin)
                {
                    //管理员或超级管理员可绕过授权验证，直接通过
                    await next();
                    return;
                }

                int id = default;
                var args = context.ActionArguments.Values;
                var firstArg = args.FirstOrDefault();

                if (FormKey is not null)
                {
                    if (context.ActionArguments.TryGetValue(FormKey, out var val))
                    {
                        if (val is int numVal)
                            id = numVal;
                    }
                }
                else if (args.Count == 1 && firstArg is int v)
                    id = v;
                else if (firstArg is IAuthGrantableRequestModel authReq)
                    id = authReq.AuthGrantOnId;
                if (id == default)
                {
                    if (IgnoreZero)
                    {
                        await next();
                        return;
                    }
                    throw new Exception("权限验证参数无效");
                }

                var controller = context.Controller as IAuthGrantTypeProvidedController;

                if(OnType == AuthGrantOn.None && controller is not null)
                    OnType = controller.AuthGrantOnType;

                if (OnType == AuthGrantOn.None && firstArg is IAuthGrantableRequstModelWithOn withOn)
                    OnType = withOn.AuthGrantOnType;

                if (OnType == AuthGrantOn.None)
                    throw new Exception("权限验证未找到匹配类型");

                if (!_authGrantService.CheckAccess(OnType, id))
                {
                    var resp = new ApiResponse(null, false, "无权限，请咨询管理员");
                    context.Result = resp.BuildResult();
                }
                else
                    await next();
            }
        }
    }

    public static class AuthGrantedAttributeExtension
    {
        public static IServiceCollection AddAuthGrantedActionFilter(this IServiceCollection services)
        {
            services.AddScoped<AuthGrantedAttribute.AuthGrantActionFilter>();
            return services;
        }
    }

    public interface IAuthGrantTypeProvidedController
    {
        public AuthGrantOn AuthGrantOnType { get; }
    }
}
