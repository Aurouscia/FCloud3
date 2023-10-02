using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FCloud3.Utils.Utils.Logger
{
    public class FLoggerSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public FLoggerFileInterval NewFileInterval { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public FLoggerLevel LogLevel { get; set; }
        public string? LogDir { get; set; }

        public static FLoggerSettings Default
        {
            get
            {
                return new FLoggerSettings { NewFileInterval = FLoggerFileInterval.Day, LogLevel = FLoggerLevel.Error ,LogDir=null};
            }
        }
    }
    public enum FLoggerFileInterval
    {
        None = 0,
        Day = 1,
        Week = 2,
        Month = 3,
    }
    public enum FLoggerLevel
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
    }
}
