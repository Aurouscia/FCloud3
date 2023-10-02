using Newtonsoft.Json;

namespace FCloud3.Utils.Utils.Settings
{
    public class SettingsAccessor<T>
    {
        private T Value { get; }
        private T? ValueDev { get; }
        public SettingsAccessor(string settingsJsonPath="appsettings.json", string settingsDevJsonPath = "appsettings.Development.json")
        {
            try
            {
                FileInfo f = new(settingsJsonPath);
                if (!f.Exists)
                    throw new Exception($"找不到配置文件{settingsJsonPath}");
                using StreamReader sr = f.OpenText();
                string settingsJson = sr.ReadToEnd();
                T? settings = JsonConvert.DeserializeObject<T>(settingsJson) ?? throw new Exception("配置文件格式异常");
                Value = settings;

                FileInfo fDev = new(settingsDevJsonPath);
                if (fDev.Exists)
                {
                    StreamReader srDev = fDev.OpenText();
                    string settingsDevJson = srDev.ReadToEnd();
                    T? settingsDev = JsonConvert.DeserializeObject<T>(settingsDevJson) ?? throw new Exception("配置文件格式异常");
                    ValueDev = settingsDev;
                }
            }
            catch
            {
                throw;
            }
        }
        public TRes? Get<TRes>(Func<T,TRes> access)
        {
            if (ValueDev is not null) {
                try
                {
                    TRes? res = access(ValueDev);
                    if (res is not null)
                        return res;
                }
                catch { }
            }
            try
            {
                return access(Value);
            }
            catch
            {
                return default;
            }
        }
    }
}
