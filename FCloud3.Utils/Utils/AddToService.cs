using Microsoft.Extensions.DependencyInjection;
using FCloud3.Utils.Utils.Logger;
using FCloud3.Utils.Utils.MemCache;
using FCloud3.Utils.Utils.Settings;

namespace FCloud3.Utils.Utils
{
    public static class AddToService
    {
        public static IServiceCollection AddUtils(this IServiceCollection services, SettingsAccessor<AppSettingsModel>? settingsAccessor = null)
        {
            //由于重启前配置都不会重新读取，所以配置读取器应该单例
            if(settingsAccessor is not null)
                services.AddSingleton<SettingsAccessor<AppSettingsModel>>(settingsAccessor);
            else
                services.AddSingleton<SettingsAccessor<AppSettingsModel>>();

            //内存缓存服务自带过期机制，应该单例
            services.AddSingleton<IMemCache<string>, MemCache<string>>();
            services.AddSingleton<IMemCache<int>, MemCache<int>>();

            //通过配置记录器里关于FLogger的配置进行初始化，为了避免发生文件争用，也应该单例
            services.AddSingleton<FLogger>(ser =>
            {
                var settingsAccessor = ser.GetService<SettingsAccessor<AppSettingsModel>>();
                if(settingsAccessor is null)
                    throw new Exception(nameof(settingsAccessor)+"为空");
                var settings = settingsAccessor.Get(s => s.FLogger);
                if(settings is not null)
                    return new(settings);
                else
                {
                    var logger = new FLogger(FLoggerSettings.Default);
                    logger.LogWarning("未找到配置","未找到关于FLogger的配置项，正在使用默认配置");
                    return logger;
                }
            });

            //jwt信息读取
            services.AddSingleton<JwtInfoProvider>();
            return services;
        }
    }
}
