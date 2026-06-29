using FCloud3.Sso;

namespace FCloud3.App.Services.Utils
{
    public class F3SsoUserInfoProvider : IUserInfoProvider
    {
        private readonly HttpUserInfoService _httpUserInfo;

        public F3SsoUserInfoProvider(HttpUserInfoService httpUserInfo)
        {
            _httpUserInfo = httpUserInfo;
        }

        public int GetUserId()
        {
            return _httpUserInfo.Id;
        }

        public string GetUserName()
        {
            return _httpUserInfo.Name;
        }

        public byte GetUserLevel()
        {
            return (byte)_httpUserInfo.Type;
        }
    }
}
