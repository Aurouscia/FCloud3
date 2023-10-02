using FCloud3.Utils.Utils.Settings;
using FCloud3.Utils.Utils.Logger;

namespace FCloud3.Utils.Utils.Settings
{
    public class JwtInfoProvider
    {
        private readonly SettingsAccessor<AppSettingsModel> _settings;
        private readonly FLogger _logger;

        public JwtInfoProvider(SettingsAccessor<AppSettingsModel> settings, FLogger logger) 
        {
            _settings = settings;
            _logger = logger;
        }
        public string GetSecretKey()
        {
            try {
                //从appsettings.json中读取key，没有就报错
                string? key = _settings.Get(x => x.SiteInfo?.JwtSecretKey)
                    ?? throw new Exception("配置文件未填写JwtSecretKey");
                return key;
            }
            catch(Exception ex)
            {
                _logger.LogError("jwt配置读取失败", ex.Message);
                throw;
            }
        }
        public string GetDomain()
        {
            try
            {
                //从appsettings.json中读取key，没有就报错
                string? key = _settings.Get(x => x.SiteInfo?.Domain);
                if(string.IsNullOrEmpty(key))
                    throw new Exception("配置文件未填写Domain");
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError("jwt配置读取失败", ex.Message);
                throw;
            }
        }
    }
}
