namespace FCloud3.App.Services.Configs
{
    //TODO 用Attribute实现可以为空的例外和其他检查

    /// <summary>
    /// 自动检查配置对象的各个属性是否未填（需要配置键与属性名一致才能正确报错）
    /// </summary>
    public static class ConfigCheck
    {
        public static void Check<T>(T configObj, string configPath)
        {
            var t = typeof(T);
            var props = t.GetProperties();
            foreach(var prop in props)
            {
                var val = prop.GetValue(configObj);
                if (prop.PropertyType == typeof(string))
                {
                    if(val == null)
                    {
                        throw new Exception($"配置项目{configPath}:{prop.Name}未填");
                    }
                    if(val.GetType() != typeof(string))
                    {
                        throw new Exception($"配置项目{configPath}:{prop.Name}应为字符串");
                    }
                    if(string.IsNullOrWhiteSpace(val as string))
                    {
                        throw new Exception($"配置项目{configPath}:{prop.Name}未填");
                    }
                }
            }
        }
    }
}
