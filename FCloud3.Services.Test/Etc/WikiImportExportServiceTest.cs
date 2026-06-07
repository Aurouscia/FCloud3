using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Entities.Table;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc.Split;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Test.TestSupport;
using Newtonsoft.Json;
using System.IO.Compression;

namespace FCloud3.Services.Test.Etc
{
    [TestClass]
    public class WikiImportExportServiceTest
    {
        private readonly FCloudContext _ctx;
        private readonly WikiImportExportService _service;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly WikiParaRepo _wikiParaRepo;
        private readonly TextSectionRepo _textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly FakeStorage _fakeStorage;

        public WikiImportExportServiceTest()
        {
            var provider = new TestingServiceProvider();
            _service = provider.Get<WikiImportExportService>();
            _wikiItemRepo = provider.Get<WikiItemRepo>();
            _wikiParaRepo = provider.Get<WikiParaRepo>();
            _textSectionRepo = provider.Get<TextSectionRepo>();
            _freeTableRepo = provider.Get<FreeTableRepo>();
            _fileItemRepo = provider.Get<FileItemRepo>();
            _fakeStorage = (FakeStorage)provider.Get<IStorage>();
            _ctx = provider.Get<FCloudContext>();

            _wikiItemRepo.ClearCache();
        }

        [TestMethod]
        public void ExportMyWikis()
        {
            SeedTestData();

            using var memStream = new MemoryStream();
            _service.ExportMyWikis(memStream, 2);
            memStream.Position = 0;

            using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);
            var entries = archive.Entries.Select(e => e.FullName).ToList();

            Assert.IsTrue(entries.Any(e => e.EndsWith(".f3w.json")), "应包含词条json文件");
            Assert.IsTrue(entries.Contains("词条附件.json"), "应包含附件汇总文件");
            Assert.IsTrue(entries.Contains("说明.txt"), "应包含说明文件");
        }

        [TestMethod]
        public void ExportMyWikis_FilterByUrlPathNames()
        {
            // 创建3个词条，用户2拥有
            var wikis = new[]
            {
                new WikiItem { Title = "词条A", UrlPathName = "wiki-a", OwnerUserId = 2 },
                new WikiItem { Title = "词条B", UrlPathName = "wiki-b", OwnerUserId = 2 },
                new WikiItem { Title = "词条C", UrlPathName = "wiki-c", OwnerUserId = 2 }
            };
            foreach (var w in wikis)
            {
                _wikiItemRepo.TryAddAndGetId(w, out _);
            }
            _ctx.ChangeTracker.Clear();

            // 只导出 wiki-a 和 wiki-c
            using var memStream = new MemoryStream();
            _service.ExportMyWikis(memStream, 2, ["wiki-a", "wiki-c"]);
            memStream.Position = 0;

            using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);
            var jsonEntries = archive.Entries
                .Where(e => e.FullName.EndsWith(".f3w.json"))
                .ToList();

            Assert.AreEqual(2, jsonEntries.Count, "应只导出2个指定词条");

            var jsonSerializer = JsonSerializer.CreateDefault();
            var titles = new List<string>();
            foreach (var entry in jsonEntries)
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                var wiki = jsonSerializer.Deserialize<WikiImportExportService.ExportedWiki>(new JsonTextReader(reader));
                titles.Add(wiki?.Info?.Title);
            }

            CollectionAssert.Contains(titles, "词条A");
            CollectionAssert.Contains(titles, "词条C");
            CollectionAssert.DoesNotContain(titles, "词条B");
        }

        [TestMethod]
        public void ExportMyWikis_FilterByUrlPathNames_EmptyList_ExportsAll()
        {
            var wikis = new[]
            {
                new WikiItem { Title = "词条A", UrlPathName = "wiki-a", OwnerUserId = 2 },
                new WikiItem { Title = "词条B", UrlPathName = "wiki-b", OwnerUserId = 2 }
            };
            foreach (var w in wikis)
            {
                _wikiItemRepo.TryAddAndGetId(w, out _);
            }
            _ctx.ChangeTracker.Clear();

            // 传入空列表，应导出全部
            using var memStream = new MemoryStream();
            _service.ExportMyWikis(memStream, 2, []);
            memStream.Position = 0;

            using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);
            var jsonEntries = archive.Entries
                .Where(e => e.FullName.EndsWith(".f3w.json"))
                .ToList();

            Assert.AreEqual(2, jsonEntries.Count, "空列表时应导出全部词条");
        }

        [TestMethod]
        public void ExportAllWikis_FilterByUrlPathNames()
        {
            // 创建跨用户的词条
            var wikis = new[]
            {
                new WikiItem { Title = "词条A", UrlPathName = "wiki-a", OwnerUserId = 2 },
                new WikiItem { Title = "词条B", UrlPathName = "wiki-b", OwnerUserId = 3 },
                new WikiItem { Title = "词条C", UrlPathName = "wiki-c", OwnerUserId = 4 }
            };
            foreach (var w in wikis)
            {
                _wikiItemRepo.TryAddAndGetId(w, out _);
            }
            _ctx.ChangeTracker.Clear();

            // uid=0 表示全部用户，但只过滤 urlPathName
            using var memStream = new MemoryStream();
            _service.ExportMyWikis(memStream, 0, ["wiki-b"]);
            memStream.Position = 0;

            using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);
            var jsonEntries = archive.Entries
                .Where(e => e.FullName.EndsWith(".f3w.json"))
                .ToList();

            Assert.AreEqual(1, jsonEntries.Count, "应只导出1个指定词条");

            var jsonSerializer = JsonSerializer.CreateDefault();
            using var stream = jsonEntries[0].Open();
            using var reader = new StreamReader(stream);
            var wiki = jsonSerializer.Deserialize<WikiImportExportService.ExportedWiki>(new JsonTextReader(reader));
            Assert.AreEqual("词条B", wiki?.Info?.Title);
        }

        [TestMethod]
        public void ExportMyWikis_FilterByUrlPathNames_NotFound_Throws()
        {
            var wikis = new[]
            {
                new WikiItem { Title = "词条A", UrlPathName = "wiki-a", OwnerUserId = 2 },
                new WikiItem { Title = "词条B", UrlPathName = "wiki-b", OwnerUserId = 2 }
            };
            foreach (var w in wikis)
            {
                _wikiItemRepo.TryAddAndGetId(w, out _);
            }
            _ctx.ChangeTracker.Clear();

            using var memStream = new MemoryStream();
            InvalidOperationException? caughtEx = null;
            try
            {
                _service.ExportMyWikis(memStream, 2, ["wiki-a", "not-exist", "wiki-c"]);
            }
            catch (InvalidOperationException ex)
            {
                caughtEx = ex;
            }

            Assert.IsNotNull(caughtEx, "应抛出 InvalidOperationException");
            StringAssert.Contains(caughtEx.Message, "not-exist");
            StringAssert.Contains(caughtEx.Message, "wiki-c");
            StringAssert.DoesNotMatch(caughtEx.Message, new System.Text.RegularExpressions.Regex("wiki-a"));
        }

        [TestMethod]
        public void ExportMyWikis_FilterByUrlPathNames_PartialNotFound_Throws()
        {
            var wikis = new[]
            {
                new WikiItem { Title = "词条A", UrlPathName = "wiki-a", OwnerUserId = 2 }
            };
            foreach (var w in wikis)
            {
                _wikiItemRepo.TryAddAndGetId(w, out _);
            }
            _ctx.ChangeTracker.Clear();

            using var memStream = new MemoryStream();
            InvalidOperationException? caughtEx = null;
            try
            {
                _service.ExportMyWikis(memStream, 2, ["wiki-a", "missing"]);
            }
            catch (InvalidOperationException ex)
            {
                caughtEx = ex;
            }

            Assert.IsNotNull(caughtEx, "应抛出 InvalidOperationException");
            StringAssert.Contains(caughtEx.Message, "missing");
        }

        [TestMethod]
        public void ImportWikis()
        {
            var exportedZip = CreateExportedZip();
            exportedZip.Position = 0;

            int count = _service.ImportWikis(exportedZip, 2, out string? errmsg);

            Assert.IsTrue(count > 0, $"应成功导入词条，错误：{errmsg}");
            Assert.IsNull(errmsg);

            var importedWiki = _wikiItemRepo.GetByUrlPathName("test-wiki").FirstOrDefault();
            Assert.IsNotNull(importedWiki, "应能找到导入的词条");
            Assert.AreEqual("测试词条", importedWiki.Title);

            var paras = _wikiParaRepo.GetParasByWikiId(importedWiki.Id).OrderBy(p => p.Order).ToList();
            Assert.AreEqual(2, paras.Count, "应包含2个段落");

            // 验证文本段落
            var textPara = paras.FirstOrDefault(p => p.Type == WikiParaType.Text);
            Assert.IsNotNull(textPara);
            var textSection = _textSectionRepo.GetById(textPara.ObjectId);
            Assert.IsNotNull(textSection);
            Assert.AreEqual("文本标题", textSection.Title);
            Assert.AreEqual("这是文本内容", textSection.Content);

            // 验证表格段落
            var tablePara = paras.FirstOrDefault(p => p.Type == WikiParaType.Table);
            Assert.IsNotNull(tablePara);
            var table = _freeTableRepo.GetById(tablePara.ObjectId);
            Assert.IsNotNull(table);
            Assert.AreEqual("表格名称", table.Name);
        }

        [TestMethod]
        public void ImportWikis_UrlPathNameConflict()
        {
            // 先创建一个同路径名的词条
            _wikiItemRepo.TryAddAndGetId(new WikiItem
            {
                Title = "已有词条",
                UrlPathName = "test-wiki"
            }, out _);
            _ctx.ChangeTracker.Clear();

            var exportedZip = CreateExportedZip();
            exportedZip.Position = 0;

            int count = _service.ImportWikis(exportedZip, 2, out string? errmsg);

            Assert.IsTrue(count > 0, $"应成功导入词条，错误：{errmsg}");

            var importedWiki = _wikiItemRepo.Existing
                .FirstOrDefault(w => w.UrlPathName == "test-wiki_1");
            Assert.IsNotNull(importedWiki, "路径名冲突时应自动添加后缀");
            Assert.AreEqual("测试词条", importedWiki.Title);
        }

        [TestMethod]
        public void ImportWikis_FilePara_DownloadAndStore()
        {
            // 预置一个"远程"文件到 FakeStorage
            var testFilePath = "wikiFile/test.png";
            var testFileBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            _fakeStorage.StoreForTest(testFilePath, testFileBytes);

            var exportedZip = CreateExportedZipWithFile(testFilePath);
            exportedZip.Position = 0;

            int count = _service.ImportWikis(exportedZip, 2, out string? errmsg);

            Assert.IsTrue(count > 0, $"应成功导入词条，错误：{errmsg}");

            var importedWiki = _wikiItemRepo.GetByUrlPathName("file-wiki").FirstOrDefault();
            Assert.IsNotNull(importedWiki);

            var filePara = _wikiParaRepo.GetParasByWikiId(importedWiki.Id)
                .FirstOrDefault(p => p.Type == WikiParaType.File);
            Assert.IsNotNull(filePara);
            Assert.IsTrue(filePara.ObjectId > 0, "文件段落应关联到有效的文件Id");
        }

        [TestMethod]
        public void ImportWikis_EmptyZip()
        {
            using var emptyZip = new MemoryStream();
            using (var archive = new ZipArchive(emptyZip, ZipArchiveMode.Create, true))
            {
            }
            emptyZip.Position = 0;

            int count = _service.ImportWikis(emptyZip, 2, out string? errmsg);

            Assert.AreEqual(0, count);
            Assert.IsNotNull(errmsg);
        }

        [TestMethod]
        public void PreviewImport()
        {
            var exportedZip = CreateExportedZip();
            exportedZip.Position = 0;

            var preview = _service.PreviewImport(exportedZip, out string? errmsg);

            Assert.IsNotNull(preview, $"预览失败：{errmsg}");
            Assert.IsNull(errmsg);
            Assert.AreEqual(1, preview.Wikis.Count, "应包含1个词条预览");

            var wikiPreview = preview.Wikis[0];
            Assert.AreEqual("测试词条", wikiPreview.Title);
            Assert.AreEqual("test-wiki", wikiPreview.OriginalUrlPathName);
            Assert.AreEqual("test-wiki", wikiPreview.ResolvedUrlPathName);
            Assert.IsFalse(wikiPreview.HasConflict, "新词条不应有冲突");
            CollectionAssert.AreEqual(new byte[] { 0, 2 }, wikiPreview.ParaTypes.ToArray());

            Assert.AreEqual(0, preview.Files.Count, "没有文件段落时应无文件");
        }

        [TestMethod]
        public void PreviewImport_WithConflict()
        {
            // 先创建同名词条
            _wikiItemRepo.TryAddAndGetId(new WikiItem
            {
                Title = "已有词条",
                UrlPathName = "test-wiki"
            }, out _);
            _ctx.ChangeTracker.Clear();

            var exportedZip = CreateExportedZip();
            exportedZip.Position = 0;

            var preview = _service.PreviewImport(exportedZip, out string? errmsg);

            Assert.IsNotNull(preview, $"预览失败：{errmsg}");
            Assert.IsNull(errmsg);
            Assert.AreEqual(1, preview.Wikis.Count);

            var wikiPreview = preview.Wikis[0];
            Assert.IsTrue(wikiPreview.HasConflict, "应有冲突标记");
            Assert.AreEqual("test-wiki", wikiPreview.OriginalUrlPathName);
            Assert.AreEqual("test-wiki_1", wikiPreview.ResolvedUrlPathName, "冲突时应显示解析后的路径名");
        }

        [TestMethod]
        public void PreviewImport_WithFile()
        {
            var exportedZip = CreateExportedZipWithFile("wikiFile/test.png");
            exportedZip.Position = 0;

            var preview = _service.PreviewImport(exportedZip, out string? errmsg);

            Assert.IsNotNull(preview, $"预览失败：{errmsg}");
            Assert.IsNull(errmsg);
            Assert.AreEqual(1, preview.Wikis.Count);
            Assert.AreEqual(1, preview.Files.Count, "应包含1个文件预览");

            var filePreview = preview.Files[0];
            Assert.AreEqual("测试文件", filePreview.DisplayName);
            Assert.AreEqual("wikiFile/test.png", filePreview.StorePathName);
            Assert.IsNotNull(filePreview.FullUrl);
        }

        [TestMethod]
        public void PreviewImport_EmptyZip()
        {
            using var emptyZip = new MemoryStream();
            using (var archive = new ZipArchive(emptyZip, ZipArchiveMode.Create, true))
            {
            }
            emptyZip.Position = 0;

            var preview = _service.PreviewImport(emptyZip, out string? errmsg);

            Assert.IsNull(preview);
            Assert.IsNotNull(errmsg);
        }

        [TestMethod]
        public void ImportWikis_FileHash_Deduplication()
        {
            // 预置文件到 FakeStorage
            var testFilePath = "wikiFile/test.png";
            var testFileBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A };
            _fakeStorage.StoreForTest(testFilePath, testFileBytes);

            // 先导入一次，创建 FileItem
            var exportedZip1 = CreateExportedZipWithFile(testFilePath);
            exportedZip1.Position = 0;
            int count1 = _service.ImportWikis(exportedZip1, 2, out string? errmsg1);
            Assert.IsTrue(count1 > 0, $"第一次导入失败：{errmsg1}");

            var importedWiki1 = _wikiItemRepo.GetByUrlPathName("file-wiki").FirstOrDefault();
            Assert.IsNotNull(importedWiki1);
            var filePara1 = _wikiParaRepo.GetParasByWikiId(importedWiki1.Id)
                .FirstOrDefault(p => p.Type == WikiParaType.File);
            Assert.IsNotNull(filePara1);
            var firstFileId = filePara1.ObjectId;
            Assert.IsTrue(firstFileId > 0);

            // 再导入一次相同文件，应该复用已有 FileItem
            var exportedZip2 = CreateExportedZipWithFile(testFilePath, "file-wiki-2");
            exportedZip2.Position = 0;
            int count2 = _service.ImportWikis(exportedZip2, 2, out string? errmsg2);
            Assert.IsTrue(count2 > 0, $"第二次导入失败：{errmsg2}");

            var importedWiki2 = _wikiItemRepo.GetByUrlPathName("file-wiki-2").FirstOrDefault();
            Assert.IsNotNull(importedWiki2);
            var filePara2 = _wikiParaRepo.GetParasByWikiId(importedWiki2.Id)
                .FirstOrDefault(p => p.Type == WikiParaType.File);
            Assert.IsNotNull(filePara2);
            var secondFileId = filePara2.ObjectId;

            // Hash 相同，应该复用同一个 FileItem
            Assert.AreEqual(firstFileId, secondFileId, "相同Hash的文件应复用已有FileItem");

            // 数据库中应该只有一个 FileItem
            var fileItems = _fileItemRepo.Existing.ToList();
            Assert.AreEqual(1, fileItems.Count, "相同Hash不应创建重复FileItem");
        }

        [TestMethod]
        public void ImportWikis_FileHash_DifferentHashCreatesNew()
        {
            // 预置两个不同内容的文件
            var testFilePath1 = "wikiFile/test1.png";
            var testFileBytes1 = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A };
            _fakeStorage.StoreForTest(testFilePath1, testFileBytes1);

            var testFilePath2 = "wikiFile/test2.png";
            var testFileBytes2 = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0B };
            _fakeStorage.StoreForTest(testFilePath2, testFileBytes2);

            // 导入第一个文件
            var exportedZip1 = CreateExportedZipWithFile(testFilePath1, "wiki-1");
            exportedZip1.Position = 0;
            int count1 = _service.ImportWikis(exportedZip1, 2, out string? errmsg1);
            Assert.IsTrue(count1 > 0, $"第一次导入失败：{errmsg1}");

            var importedWiki1 = _wikiItemRepo.GetByUrlPathName("wiki-1").FirstOrDefault();
            Assert.IsNotNull(importedWiki1);
            var filePara1 = _wikiParaRepo.GetParasByWikiId(importedWiki1.Id)
                .FirstOrDefault(p => p.Type == WikiParaType.File);
            Assert.IsNotNull(filePara1);
            var firstFileId = filePara1.ObjectId;

            // 导入第二个文件（不同内容）
            var exportedZip2 = CreateExportedZipWithFile(testFilePath2, "wiki-2");
            exportedZip2.Position = 0;
            int count2 = _service.ImportWikis(exportedZip2, 2, out string? errmsg2);
            Assert.IsTrue(count2 > 0, $"第二次导入失败：{errmsg2}");

            var importedWiki2 = _wikiItemRepo.GetByUrlPathName("wiki-2").FirstOrDefault();
            Assert.IsNotNull(importedWiki2);
            var filePara2 = _wikiParaRepo.GetParasByWikiId(importedWiki2.Id)
                .FirstOrDefault(p => p.Type == WikiParaType.File);
            Assert.IsNotNull(filePara2);
            var secondFileId = filePara2.ObjectId;

            // Hash 不同，应该创建不同的 FileItem
            Assert.AreNotEqual(firstFileId, secondFileId, "不同Hash的文件应创建新的FileItem");

            // 数据库中应该有两个 FileItem
            var fileItems = _fileItemRepo.Existing.ToList();
            Assert.AreEqual(2, fileItems.Count, "不同Hash应创建独立的FileItem");
        }

        private void SeedTestData()
        {
            var wiki = new WikiItem
            {
                Title = "导出测试",
                UrlPathName = "export-test",
                OwnerUserId = 2
            };
            _wikiItemRepo.TryAddAndGetId(wiki, out _);

            var text = new FCloud3.Entities.TextSection.TextSection { Title = "文本", Content = "内容" };
            _textSectionRepo.AddDefaultAndGetId();

            _ctx.ChangeTracker.Clear();
        }

        private MemoryStream CreateExportedZip()
        {
            var exportedWiki = new WikiImportExportService.ExportedWiki(new WikiItem
            {
                Title = "测试词条",
                UrlPathName = "test-wiki",
                Description = "测试描述",
                LastActive = new DateTime(2024, 6, 1)
            });
            exportedWiki.Paras.Add(new WikiImportExportService.ExportedWiki.ExportedWikiPara(
                "文本标题", null, WikiParaType.Text, "这是文本内容"));
            exportedWiki.Paras.Add(new WikiImportExportService.ExportedWiki.ExportedWikiPara(
                "表格名称", null, WikiParaType.Table,
                "{\"name\":\"表格名称\",\"cells\":[[\"a\",\"b\"],[\"c\",\"d\"]],\"merges\":[]}"));
            // 不添加 File 段落，避免测试中需要真实 HTTP 下载
            return CreateZipWithWiki(exportedWiki);
        }

        private MemoryStream CreateExportedZipWithFile(string filePathName, string? urlPathName = null)
        {
            var exportedWiki = new WikiImportExportService.ExportedWiki(new WikiItem
            {
                Title = "文件词条",
                UrlPathName = urlPathName ?? "file-wiki",
                Description = null,
                LastActive = DateTime.Now
            });
            exportedWiki.Paras.Add(new WikiImportExportService.ExportedWiki.ExportedWikiPara(
                "测试文件", null, WikiParaType.File, filePathName));

            return CreateZipWithWiki(exportedWiki, "");
        }

        private static MemoryStream CreateZipWithWiki(WikiImportExportService.ExportedWiki wiki, string? urlBase = null)
        {
            var memStream = new MemoryStream();
            using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, true))
            {
                var jsonSerializer = JsonSerializer.CreateDefault();
                jsonSerializer.Formatting = Formatting.Indented;

                var entry = archive.CreateEntry("词条/test.f3w.json");
                using (var stream = entry.Open())
                {
                    using var writer = new StreamWriter(stream);
                    jsonSerializer.Serialize(writer, wiki);
                    writer.Flush();
                }

                if (urlBase is not null)
                {
                    var attachmentsEntry = archive.CreateEntry("词条附件.json");
                    using (var attachmentsStream = attachmentsEntry.Open())
                    {
                        using var attachmentsWriter = new StreamWriter(attachmentsStream);
                        var attachments = new WikiImportExportService.AttachmentsSummary(urlBase);
                        jsonSerializer.Serialize(attachmentsWriter, attachments);
                        attachmentsWriter.Flush();
                    }
                }
            }
            return memStream;
        }
    }
}
