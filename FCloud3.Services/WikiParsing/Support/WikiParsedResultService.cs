using Microsoft.Extensions.Configuration;
using System.IO;

namespace FCloud3.Services.WikiParsing.Support
{
    /// <summary>
    /// 词条解析结果存储，单例服务
    /// </summary>
    public class WikiParsedResultService
    {
        private readonly string _path;
        private readonly Random r = new();
        public WikiParsedResultService(IConfiguration config)
        {
            _path = config["ParsedResult:Path"]
                ?? throw new Exception("解析结果存储路径未填");
            var dir = new DirectoryInfo(_path);
            if (!dir.Exists)
                dir.Create();
        }
        public Stream Save(int wikiId)
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
                //保留最新的一个版本，删掉前面的
                else
                    file.Delete();
            }
            int version = r.Next(100000,999999);
            var stream = File.Open($"{dirPath}/{version}.json", FileMode.Create, FileAccess.Write, FileShare.None);
            // FileShare.None 不允许没写完之前读取
            return stream;
        }
        public Stream? Read(int wikiId)
        {
            var dirPath = GetTargetDir(wikiId);
            var dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
                dir.Create();
            var versions = dir.GetFiles().OrderByDescending(f => f.CreationTime);
            foreach(var version in versions)
            {
                try
                {
                    //如果打开失败，尝试更旧的版本
                    return version.OpenRead();
                }
                catch { }
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
    }
}
