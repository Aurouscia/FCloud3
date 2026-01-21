using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace FCloud3.Services.Etc.Split
{
    public class WikiImportExportService(
        WikiItemRepo wikiItemRepo,
        WikiParaRepo wikiParaRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo,
        FileItemRepo fileItemRepo,
        WikiRefRepo wikiRefRepo,
        MaterialRepo materialRepo,
        IStorage storage)
    {
        /// <summary>
        /// 导出当前用户的所有词条及其中引用的文件
        /// 运行此功能假定当前WikiRefs状态完整
        /// 如果不能保证WikiRefs完整，应先让版主运行InitController中的EnforceParseAll方法
        /// </summary>
        /// <param name="memStream">zip写入的MemoryStream</param>
        /// <param name="uid">用户id，为0表示全部用户</param>
        public void ExportMyWikis(MemoryStream memStream, int uid)
        {
            List<WikiItem> myWikis;
            if (uid > 0)
            {
                myWikis = wikiItemRepo.Existing
                    .Where(x => x.OwnerUserId == uid)
                    .ToList();
            }
            else
            {
                myWikis = wikiItemRepo.Existing.ToList();
            }
            
            var allParas = wikiParaRepo.Existing
                .Select(x => new { x.WikiItemId, x.NameOverride, x.ObjectId, x.Type, x.Order })
                .ToList();
            var myWikiIds = myWikis.Select(x => x.Id).ToList();
            var myParas = allParas.Where(x => myWikiIds.Contains(x.WikiItemId));
            var myTextIds = myParas
                .Where(x => x.Type == WikiParaType.Text)
                .Select(x => x.ObjectId).ToList();
            var myTableIds = myParas
                .Where(x => x.Type == WikiParaType.Table)
                .Select(x => x.ObjectId).ToList();
            var myFileIds = myParas
                .Where(x => x.Type == WikiParaType.File)
                .Select(x => x.ObjectId).ToList();
            var myTexts = textSectionRepo.Existing
                .Where(x => myTextIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Title, x.Content }).ToList();
            var myTables = freeTableRepo.Existing
                .Where(x => myTableIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name, x.Data }).ToList();
            var myFiles = fileItemRepo.Existing
                .Where(x => myFileIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName, x.StorePathName }).ToList();

            var exportedWikis = new List<ExportedWiki>();
            var attachments = new AttachmentsSummary(storage.GetUrlBase());
            foreach (var w in myWikis)
            {
                var exw = new ExportedWiki(w);
                var itsParas = allParas
                    .Where(x => x.WikiItemId == w.Id)
                    .OrderBy(x => x.Order);
                foreach (var p in itsParas)
                {
                    ExportedWiki.ExportedWikiPara? para = null;
                    if (p.Type == WikiParaType.Text)
                    {
                        var text = myTexts.Find(x => x.Id == p.ObjectId);
                        if (text is { })
                            para = new(text.Title, p.NameOverride, p.Type, text.Content);
                    }
                    else if (p.Type == WikiParaType.Table)
                    {
                        var table = myTables.Find(x => x.Id == p.ObjectId);
                        if (table is { })
                            para = new(table.Name, p.NameOverride, p.Type, table.Data);
                    }
                    else if (p.Type == WikiParaType.File)
                    {
                        var file = myFiles.Find(x => x.Id == p.ObjectId);
                        if (file is { })
                        {
                            para = new(file.DisplayName, p.NameOverride, p.Type, file.StorePathName);
                            if (file.StorePathName is { })
                                attachments.FileItems.Add(file.StorePathName);
                        }
                    }
                    if (para is { })
                        exw.Paras.Add(para);
                }
                var refTexts = wikiRefRepo.WikiRefs
                    .Where(x => x.WikiId == w.Id)
                    .Select(x => x.Str).ToList();
                var refedMats = materialRepo
                    .CachedItemsByPred(x => refTexts.Contains(x.Name))
                    .Select(x => x.PathName);
                foreach (var mat in refedMats)
                {
                    if (mat is { })
                        attachments.Materials.Add(mat);
                }
                exportedWikis.Add(exw);
            }

            using var archive = new ZipArchive(memStream, ZipArchiveMode.Create, true);
            var jsonSerializer = JsonSerializer.CreateDefault();
            jsonSerializer.Formatting = Formatting.Indented;
            foreach (var w in exportedWikis)
            {
                var fileName = GetZipEntryNameForWiki(w.Info.Title);
                var wikiEntry = archive.CreateEntry(fileName);
                using var stream = wikiEntry.Open();
                using var writer = new StreamWriter(stream);
                jsonSerializer.Serialize(writer, w);
                writer.Flush();
            }
            string attachmentsEntryName = "词条附件.json";
            var attachmentsEntry = archive.CreateEntry(attachmentsEntryName);
            using (var attachmentsStream = attachmentsEntry.Open())
            {
                using var attachmentsWriter = new StreamWriter(attachmentsStream);
                jsonSerializer.Serialize(attachmentsWriter, attachments);
                attachmentsWriter.Flush();
            }

            var readmeEntry = archive.CreateEntry("说明.txt");
            using (var readmeStream = readmeEntry.Open())
            {
                using var readmeWriter = new StreamWriter(readmeStream);
                var readmeContent = $"""
                你好：
                本压缩包内是你的所有词条，以及它们引用的文件链接。
                如需将内容导入另一个FCloud3网站，把本压缩包交给该网站管理员。
                请注意，本压缩包仅备份了文本内容，词条中引用的文件依然在原有服务器上。
                如果需要下载到本地，见"{attachmentsEntryName}"文件。
                在另一网站导入时，新网站会自动获取和存储这些引用文件，无需任何操作。

                除非你知道你在做什么，否则不要随意修改文件中的任何内容，以免造成格式损坏。
                """;
                readmeWriter.Write(readmeContent);
                readmeWriter.Flush();
            }

            archive.Dispose();
        }


        private readonly static string fileNameInvalidChars
            = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        private readonly static string fileNameRegexInvalidPattern
            = $"[{fileNameInvalidChars}]";
        private const string wikiDirNameInZip = "词条";
        private static string GetZipEntryNameForWiki(string? wTitle)
        {
            var fileExt = Path.GetRandomFileName();
            fileExt = Path.ChangeExtension(fileExt, ".f3w.json");
            wTitle ??= "";
            if(wTitle.Length > 32)
                wTitle = wTitle[..32];
            var fileName = Path.ChangeExtension(wTitle, fileExt);
            
            string validFileName = Regex.Replace(fileName, fileNameRegexInvalidPattern, "_");
            return $"{wikiDirNameInZip}/{validFileName}";
        }

        public class ExportedWiki(WikiItem info)
        {
            public ExportedWikiInfo Info { get; } = new(info);
            public List<ExportedWikiPara> Paras { get; } = [];

            public class ExportedWikiInfo(WikiItem infoSource)
            {
                public string? Title { get; } = infoSource.Title;
                public string? UrlPathName { get; } = infoSource.UrlPathName;
                public string? Description { get; } = infoSource.Description;
                public DateTime LastActive { get; } = infoSource.LastActive;
            }
            public class ExportedWikiPara(
                string? objName, string? paraName, WikiParaType paraType, string? data)
            {
                public string? ObjName { get; } = objName;
                public string? ParaName { get; } = paraName;
                public WikiParaType ParaType { get; } = paraType;
                public string? Data { get; } = data;
            }
        }
        public class AttachmentsSummary(string urlBase)
        {
            public string UrlBase { get; } = urlBase;
            public HashSet<string> Materials { get; } = [];
            public HashSet<string> FileItems { get; } = [];
        }
    }
}
