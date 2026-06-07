using FCloud3.DbContexts;
using FCloud3.Entities.Table;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
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
        private readonly FakeStorage _fakeStorage;

        public WikiImportExportServiceTest()
        {
            var provider = new TestingServiceProvider();
            _service = provider.Get<WikiImportExportService>();
            _wikiItemRepo = provider.Get<WikiItemRepo>();
            _wikiParaRepo = provider.Get<WikiParaRepo>();
            _textSectionRepo = provider.Get<TextSectionRepo>();
            _freeTableRepo = provider.Get<FreeTableRepo>();
            _fakeStorage = (FakeStorage)provider.Get<IStorage>();
            _ctx = provider.Get<FCloudContext>();
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

        private MemoryStream CreateExportedZipWithFile(string filePathName)
        {
            var exportedWiki = new WikiImportExportService.ExportedWiki(new WikiItem
            {
                Title = "文件词条",
                UrlPathName = "file-wiki",
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
