using Aurouscia.FicKit.Common.Abstractions;
using FCloud3.App.Services.Utils;

namespace FCloud3.App.Services.FicKit
{
    public class FicKitUserInfoProvider(
        HttpUserInfoService httpUserInfoService
        ) : IUserInfoProvider
    {
        public string? GetUserAvatarUrl()
        {
            return httpUserInfoService.AvtSrc;
        }

        public int GetUserId()
        {
            return httpUserInfoService.Id;
        }

        public int GetUserLevel()
        {
            return (int)httpUserInfoService.Type;
        }

        public string? GetUserName()
        {
            return httpUserInfoService.Name;
        }
    }
}
