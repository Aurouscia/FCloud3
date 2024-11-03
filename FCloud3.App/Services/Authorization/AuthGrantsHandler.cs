using FCloud3.Entities.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;

namespace FCloud3.App.Services.Auth
{
    //留着作参考，免得忘了自定义Authorization怎么写
    [Obsolete("已经改用AuthGrantActionFilter进行验证")]
    public class AuthGrantsHandler : AuthorizationHandler<AuthGrantsRequirement>
    {
        private readonly AuthGrantService _authGrantService;
        private readonly AuthGrantsOnIdProvider _authGrantsOnIdProvider;

        public AuthGrantsHandler(AuthGrantService authGrantService, AuthGrantsOnIdProvider authGrantsOnIdProvider)
        {
            _authGrantService = authGrantService;
            _authGrantsOnIdProvider = authGrantsOnIdProvider;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthGrantsRequirement requirement)
        {
            var ctx = context.Resource as DefaultHttpContext;
            if (ctx is not null)
            {
                ctx.Request.RouteValues.TryGetValue("controller", out object? controller);
                ctx.Request.RouteValues.TryGetValue("action", out object? action);
                string? controllerName = controller?.ToString()?.ToLower();
                string? actionName = action?.ToString();
                int id = _authGrantsOnIdProvider.Get();
                if(!string.IsNullOrWhiteSpace(controllerName) && !string.IsNullOrWhiteSpace(actionName) && id>0)
                {
                    AuthGrantOn onType = AuthGrantOn.None;
                    switch (controllerName)
                    {
                        case "filedir":
                            onType = AuthGrantOn.Dir;break;
                        case "wikiitem":
                            onType = AuthGrantOn.WikiItem;break;
                    }
                    if (_authGrantService.CheckAccess(onType, id))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                    else
                    {
                        context.Fail();
                        return Task.CompletedTask;
                    }
                }
                else
                    throw new Exception($"缺少RouteValues");
            }
            else
                throw new Exception($"未能取得{nameof(DefaultHttpContext)}对象");
        }
    }
}
