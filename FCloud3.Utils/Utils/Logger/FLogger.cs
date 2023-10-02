using FCloud3.Utils.Utils.FileUtils;

namespace FCloud3.Utils.Utils.Logger
{
    public class FLogger
    {
        private StreamWriter _writer;
        private DateTime _currentLogFileCreated;
        private Func<bool> _tooOld;
        private FLoggerSettings _settings; 

        private const string defaultLogDir = "./Logs";
        public FLogger(FLoggerSettings settings)
        {
            _settings = settings;
            InitLogger(settings);
            if (_writer is null || _tooOld is null)
                throw new Exception("日志初始化失败");
        }
        ~FLogger()
        {
            _writer.Flush();
            _writer.Dispose();
        }

        private void InitLogger(FLoggerSettings settings)
        {
            string logDirPath = settings.LogDir ?? defaultLogDir;
            DirectoryInfo logDir = new(logDirPath);

            DateTime newest = DateTime.MinValue;
            FileInfo? newestLogFile = null;
            if (!logDir.Exists)
                logDir.Create();
            else
            {
                foreach(var f in logDir.EnumerateFiles())
                {
                    if (DateTime.TryParse(f.Name.WithoutSuffix(), out DateTime fileTime))
                        if (fileTime > newest)
                        {
                            newest = fileTime;
                            newestLogFile = f;
                        }
                }
            }
            bool needNewLogFile = false;
            switch (settings.NewFileInterval)
            {
                default:
                case FLoggerFileInterval.None:
                case FLoggerFileInterval.Day:
                    _tooOld = () => (DateTime.Now - _currentLogFileCreated) > TimeSpan.FromDays(1);
                    break;
                case FLoggerFileInterval.Week:
                    _tooOld = () => (DateTime.Now - _currentLogFileCreated) > TimeSpan.FromDays(7);
                    break;
                case FLoggerFileInterval.Month:
                    _tooOld = () => (_currentLogFileCreated.Year != DateTime.Now.Year && _currentLogFileCreated.Month != DateTime.Now.Month);
                    break;
            }
            if (needNewLogFile || newestLogFile is null)
            {
                string fileName = $"{DateTime.Now.ToString("yyyy-M-d")}.html";
                string newFilePath = Path.Combine(logDir.FullName, fileName);
                var stream = File.Create(newFilePath);
                _writer = new(stream);
                _writer.WriteLine("<style>.err{background-color:red;color:white} .warn{background-color:yellow} h2{border-bottom:1px solid #000}</style>");
                _currentLogFileCreated = DateTime.Now;
            }
            else
            {
                _writer = new(newestLogFile.FullName, true);
                _currentLogFileCreated = newest;
            }
        }

        private void CheckLogFile()
        {
            if (_tooOld())
            {
                _writer.Flush();
                _writer.Dispose();
                InitLogger(_settings);
            }
        }

        public void LogError(string title, string msg)
        {
            CheckLogFile();
            _writer.WriteLine("<div class=\"err\">");
            _writer.WriteLine($"<h2>【{title}】</h2>");
            _writer.WriteLine(msg);
            _writer.WriteLine("</div>");
            _writer.Flush();
        }

        public void LogWarning(string title, string msg)
        {
            if (_settings.LogLevel == FLoggerLevel.Error)
                return;
            CheckLogFile();
            _writer.WriteLine("<div class=\"warn\">");
            _writer.WriteLine($"<h2>【{title}】</h2>");
            _writer.WriteLine(msg);
            _writer.WriteLine("</div>");
            _writer.Flush();
        }

        public void LogInfo(string title, string msg)
        {
            if (_settings.LogLevel == FLoggerLevel.Error || _settings.LogLevel == FLoggerLevel.Warning)
                return;
            CheckLogFile();
            _writer.WriteLine("<div>");
            _writer.WriteLine($"<h2>【{title}】</h2>");
            _writer.WriteLine(msg);
            _writer.WriteLine("</div>");
            _writer.Flush();
        }
    }
}