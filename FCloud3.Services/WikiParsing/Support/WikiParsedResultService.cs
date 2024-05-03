using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FCloud3.Services.WikiParsing.Support
{
    /// <summary>
    /// 词条解析结果存储，单例服务
    /// </summary>
    public class WikiParsedResultService
    {
        private readonly string _path;
        public WikiParsedResultService(IConfiguration config)
        {
            _path = config["ParsedResult:Path"]
                ?? throw new Exception("解析结果存储路径未填");
            var dir = new DirectoryInfo(_path);
            if (!dir.Exists)
                dir.Create();
        }
        public Stream Save(int wikiId, DateTime update)
        {
            var dirPath = GetTargetDir(wikiId);
            var dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
                dir.Create();
            var files = dir.GetFiles().OrderByDescending(f => f.CreationTime);
            bool newest = true;
            foreach (var file in files)
            {
                if (newest)
                    newest = false;
                //保留最新的一个版本，删掉前面的，确保随时都有东西可返回
                else
                {
                    //避免试图删正在读取的抛出错误
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
            }
            string fileName = FileName(update);
            var stream = File.Open($"{dirPath}/{fileName}", FileMode.Create, FileAccess.Write, FileShare.None);
            // FileShare.None 不允许没写完之前读取
            return stream;
        }
        public Stream? Read(int wikiId, DateTime update)
        {
            var dirPath = GetTargetDir(wikiId);
            var dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
                dir.Create();
            var versions = dir.GetFiles().OrderByDescending(f => f.CreationTime);
            var newest = versions.FirstOrDefault();
            if (newest is null || newest.Name != FileName(update))
                return null; //没有文件或没有最新版文件
            foreach(var version in versions)
            {
                try
                {
                    return version.OpenRead();
                }
                catch 
                {
                    //如果打开失败，尝试更旧的版本，但如果失败的有点离谱就报错
                    if ((DateTime.Now - version.CreationTime).TotalSeconds > 30)
                        throw;
                }
            }
            return null;
        }
        private string GetTargetDir(int wikiId)
        {
            int subDirId = wikiId / 1000;
            string subDirName = $"{subDirId}-{subDirId + 1}";
            var subDir = new DirectoryInfo($"{_path}/{subDirName}");
            if (!subDir.Exists)
                subDir.Create();
            string mydirName = $"{_path}/{subDirName}/{wikiId}";
            return mydirName;
        }
        private static string FileName(DateTime lastUpdate)
        {
            if (lastUpdate < UpdateTimeLowerConstraint)
                lastUpdate = UpdateTimeLowerConstraint;
            return new DateTimeOffset(lastUpdate).ToUnixTimeSeconds().ToString()+".json";
        }

        private static readonly DateTime UpdateTimeLowerConstraint = new DateTime(1980, 1, 1);
    }
}
