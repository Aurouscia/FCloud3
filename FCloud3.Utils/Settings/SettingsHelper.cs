using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Utils.Settings
{
    public class SettingsHelper
    {
        public static IConfiguration? Configuration { get; private set; }
        public SettingsHelper(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public static string? Get(params string[] sections)
        {
            try
            {

                if (sections.Any() && Configuration is not null)
                {
                    string key = string.Join(':', sections);
                    string? res = Configuration[key];
                    return res;
                }
            }
            catch { }

            return null;
        }
        public static T? GetEnum<T>(T? defaultValue = default, params string[] sections) where T : Enum
        {
            string? value = Get(sections);
            if (value is null)
                return defaultValue;
            Type type = typeof(T);
            var names = type.GetEnumNames();
            if (names.Contains(value))
            {
                var ts = type.GetEnumValues().OfType<T>();
                return ts.First(x => x.ToString() == value);
            }
            return defaultValue;
        }
    }
}
