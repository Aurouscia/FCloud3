using FCloud3.Repos.Identities;
using FCloud3.Services.Identities;

namespace FCloud3.App.Services
{
    public class HttpUserInfoService
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public int LeftHours { get; set; } = 0;
        public HttpUserInfoService(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            var ctx = httpContextAccessor.HttpContext;
            if (ctx is null)
                return;
            var idClaim = ctx.User.Claims.FirstOrDefault(x => x.Type.Contains("claims/nameidentifier"));
            var expClaim = ctx.User.Claims.FirstOrDefault(x => x.Type == "exp");
            if (idClaim is null)
                return;
            else
            {
                if (int.TryParse(idClaim.Value, out int id))
                {
                    var user = userService.GetById(id);
                    if (user is not null)
                    {
                        Id = id;
                        Name = user.Name??"";
                    }
                }
                if (expClaim is not null && long.TryParse(expClaim.Value, out long exp))
                {
                    long now = DateTimeOffset.Now.ToUnixTimeSeconds();
                    long leftSeconds = exp - now;
                    LeftHours = (int)TimeSpan.FromSeconds(leftSeconds).TotalHours;
                }
            }
        }
    }
}
