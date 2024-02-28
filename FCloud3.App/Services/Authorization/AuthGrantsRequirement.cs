using Microsoft.AspNetCore.Authorization;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace FCloud3.App.Services.Auth
{
    [Obsolete("已经改用AuthGrantActionFilter进行验证")]
    public class AuthGrantsRequirement : IAuthorizationRequirement
    {
        public const string routeValueKey = "agi";
        public const string Policy = "AuthGrantsPolicy";
    }
    [Obsolete("已经改用AuthGrantActionFilter进行验证")]
    public class AuthGrantsOnIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int Id { get; set; }

        public AuthGrantsOnIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public int Get()
        {
            if (Id > 0)
                return Id;
            if (_httpContextAccessor.HttpContext is not null)
            {
                _httpContextAccessor.HttpContext.Request.Query.TryGetValue(AuthGrantsRequirement.routeValueKey, out var idObj);
                if (int.TryParse(idObj.ToString(), out int id))
                {
                    Id = id;
                    return id;
                }
            }
            return 0;
        }
        public void Check(int id)
        {
            if (id != Get())
                throw new Exception("数据异常");
        }
    }
    [Obsolete("已经改用AuthGrantActionFilter进行验证")]
    public static class AuthGrantsPolicyExtension
    {
        public static AuthorizationOptions AddAuthGrants(this AuthorizationOptions options)
        {
            var req = new AuthGrantsRequirement();
            options.AddPolicy(AuthGrantsRequirement.Policy , policy =>
            {
                policy.AddRequirements(req);
            });
            return options;
        }
    }
}
