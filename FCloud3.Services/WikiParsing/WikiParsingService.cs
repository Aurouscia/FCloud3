using Aurouscia.TableEditor.Core;
using Aurouscia.TableEditor.Core.Html;
using FCloud3.Entities.Files;
using FCloud3.Entities.Table;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using FCloud3.HtmlGen.Context.SubContext;
using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Rules;
using FCloud3.Repos.Files;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc.Metadata;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Wiki;
using FCloud3.Services.WikiParsing.Support;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FCloud3.Services.WikiParsing
{
    public class WikiParsingService(
        WikiItemRepo wikiItemRepo,
        WikiItemMetadataService wikiItemMetadataService,
        WikiParaRepo wikiParaRepo,
        WikiTitleContainRepo wikiTitleContainRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo,
        FileItemRepo fileItemRepo,
        WikiParserProviderService wikiParserProvider,
        WikiParsedResultService wikiParsedResult,
        IStorage storage,
        ILogger<WikiParsingService> logger)
    {
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiItemMetadataService _wikiItemMetadataService = wikiItemMetadataService;
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly WikiTitleContainRepo _wikiTitleContainRepo = wikiTitleContainRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;
        private readonly FileItemRepo _fileItemRepo = fileItemRepo;
        private readonly WikiParserProviderService _wikiParserProvider = wikiParserProvider;
        private readonly WikiParsedResultService _wikiParsedResult = wikiParsedResult;
        private readonly IStorage _storage = storage;
        private readonly ILogger<WikiParsingService> _logger = logger;

        public Stream? GetParsedWikiStream(string pathName)
        {
            var w = _wikiItemMetadataService.Get(pathName);
            if (w is null)
                return null;
            return GetParsedWikiStream(w.Id, w.Update);
        }
        public Stream GetParsedWikiStream(int id, DateTime update)
        {
            lock (GetLockObj(id))
            {
                Stream? stream = _wikiParsedResult.Read(id, update);
                if (stream is not null)
                {
                    _logger.LogInformation("提供[{id}]号词条，缓存命中", id);
                    return stream;
                }
                _logger.LogInformation("提供[{id}]号词条，缓存未命中", id);
                var res = GetParsedWiki(id);

                using (var resultFileStream = _wikiParsedResult.Save(id, update))
                {
                    using var streamWriter = new StreamWriter(resultFileStream);
                    using var jsonWriter = new JsonTextWriter(streamWriter);
                    JsonSerializer serializer = new();
                    serializer.Serialize(jsonWriter, res);
                    jsonWriter.Flush();
                }
                _logger.LogInformation("提供[{id}]号词条，解析和储存完成", id);
                return _wikiParsedResult.Read(id, update) ?? throw new Exception("结果文件写入失败");
            }
        }


        public WikiParsingResult GetParsedWiki(string pathName)
        {
            var w =  _wikiItemRepo.GetByUrlPathName(pathName).FirstOrDefault();
            if (w is null)
                return WikiParsingResult.FallToInstance;
            return GetParsedWiki(w);
        }
        public WikiParsingResult GetParsedWiki(int wikiId)
        {
            var w = _wikiItemRepo.GetById(wikiId);
            if (w is null)
                return WikiParsingResult.FallToInstance;
            return GetParsedWiki(w);
        }
        private WikiParsingResult GetParsedWiki(WikiItem wiki)
        {
            var paras = _wikiParaRepo.Existing
                .Where(x => x.WikiItemId == wiki.Id)
                .OrderBy(x => x.Order)
                .ToList();

            List<int> textIds = paras.Where(x => x.Type == WikiParaType.Text).Select(x => x.ObjectId).ToList();
            List<TextSection> textParaObjs = _textSectionRepo.GetRangeByIds(textIds).ToList();
            List<int> fileIds = paras.Where(x => x.Type == WikiParaType.File).Select(x => x.ObjectId).ToList();
            List<FileItem> fileParaObjs = _fileItemRepo.GetRangeByIds(fileIds).ToList();
            List<int> tableIds = paras.Where(x => x.Type == WikiParaType.Table).Select(x => x.ObjectId).ToList();
            List<FreeTable> tableParaObjs = _freeTableRepo.GetRangeByIds(tableIds).ToList();
            List<WikiTitleContain> textContains = _wikiTitleContainRepo.GetByTypeAndObjIds(WikiTitleContainType.TextSection, textIds);
            List<WikiTitleContain> tableContains = _wikiTitleContainRepo.GetByTypeAndObjIds(WikiTitleContainType.FreeTable, tableIds);

            var contains = textContains.UnionBy(tableContains, x => x.WikiId).ToList();
            var parser = _wikiParserProvider.Get($"w_{wiki.Id}", 
                configure: builder => builder.Cache.DisableCache(),
                contains,
                true,
                () => [wiki.Id]);
            parser.Context.Reset(true);

            WikiParsingResult result = new()
            {
                Title = wiki.Title ?? "??",
            };
            paras.ForEach(p =>
            {
                if (p.Type == WikiParaType.Text)
                {
                    TextSection? model = textParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    var resOfP = ParseText(model, parser);
                    parser.WrapSection(model.Title, resOfP.Titles, out var title, out int titleId);
                    result.SubTitles.Add(title);
                    result.AddRules(resOfP.UsedRulesWithCommons);
                    result.FootNotes.AddRange(resOfP.FootNotes);
                    result.Paras.Add(new(model.Title, titleId, resOfP.Content, p.Id, p.Type, p.ObjectId, 0));
                }
                else if (p.Type == WikiParaType.Table)
                {
                    FreeTable? model = tableParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    var resOfP = ParseTable(model, parser);
                    parser.WrapSection(model.Name, resOfP.Titles, out var title, out int titleId);
                    result.SubTitles.Add(title);
                    result.AddRules(resOfP.UsedRulesWithCommons);
                    result.FootNotes.AddRange(resOfP.FootNotes);
                    result.Paras.Add(new(model.Name, titleId, resOfP.Content, p.Id, p.Type, p.ObjectId, 0));
                }
                else if (p.Type == WikiParaType.File)
                {
                    FileItem? model = fileParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    result.Paras.Add(new(model.DisplayName, 0, _storage.FullUrl(model.StorePathName ?? "??"), p.Id, p.Type, p.ObjectId, model.ByteCount));
                }
            });
            result.ExtractRulesCommon();
            return result;
        }
        private static ParserResultRaw ParseText(TextSection model, Parser parser)
        {
            return parser.RunToParserResultRaw(model.Content);
        }
        private static ParserResultRaw ParseTable(FreeTable model, Parser parser)
        {
            AuTable data = model.GetData();
            List<IRule> usedRules = new();
            Func<string?, string> cellConverter;
            if (data.Cells is not null && data.Cells.ConvertAll(x => x?.Count).Sum() <= 100)
                cellConverter = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return "　";
                    var res = parser.RunToParserResultRaw(s, false);
                    usedRules.AddRange(res.UsedRules);
                    return res.Content;
                };
            else
                cellConverter = x => x ?? "　";
            var html = data.ConvertToHtml(new()
            {
                CellConverter = cellConverter
            });
            return new(html, usedRules);
        }

        private readonly static Dictionary<int, object> lockObjs = [];
        private static object GetLockObj(int id)
        {
            if (lockObjs.TryGetValue(id, out object? obj))
            {
                return obj;
            }
            obj = new();
            lockObjs.Add(id, obj);
            return obj;
        }

        public class WikiParsingResult
        {
            public string Title { get; set; }
            public List<string> UsedRules { get; set; }
            public List<string> FootNotes { get; set; }
            public List<ParserTitleTreeNode> SubTitles { get; set; }
            public List<WikiParsingResultItem> Paras { get; set; }
            public string? Styles { get; set; }
            public string? PreScripts { get; set; }
            public string? PostScripts { get; set; }
            private List<IRule> UsedRulesBody { get; set; } 
            public WikiParsingResult()
            {
                Title = "";
                UsedRules = [];
                FootNotes = [];
                SubTitles = [];
                Paras = [];
                UsedRulesBody = [];
            }
            public void AddRules(List<IRule> rules)
            {
                rules.ForEach(x =>
                {
                    if(!UsedRules.Any(r => r == x.UniqueName))
                    {
                        UsedRules.Add(x.UniqueName);
                        UsedRulesBody.Add(x);
                    }    
                });
            }
            public void ExtractRulesCommon()
            {
                Styles = string.Join("\n\n", UsedRulesBody.ConvertAll(x => x.GetStyles()));
                PreScripts = string.Join("\n\n", UsedRulesBody.ConvertAll(x => x.GetPreScripts()));
                PostScripts = string.Join("\n\n", UsedRulesBody.ConvertAll(x => x.GetPostScripts()));
            }
            public class WikiParsingResultItem
            {
                public string? Title { get; set; }
                public int TitleId { get; set; }
                public string? Content { get; set; }
                public int ParaId { get; set; }
                public WikiParaType ParaType { get; set; }
                public int UnderlyingId { get; set; }
                public int Bytes { get; set; }
                public WikiParsingResultItem(string? title,int titleId, string content, int paraId, WikiParaType type, int underlyingId, int bytes)
                {
                    Title = title;
                    TitleId = titleId;
                    Content = content;
                    ParaType = type;
                    ParaId = paraId;
                    UnderlyingId = underlyingId;
                    Bytes = bytes;
                }
            }
            public static WikiParsingResult FallToInstance
            {
                get {
                    return new WikiParsingResult()
                    {
                        Paras = new()
                        {
                            new("找不到指定路径名的词条", 0, "可能是词条不存在或已被移走，请确认后重试", 0, WikiParaType.Text, 0, 0)
                        }
                    };
                }
            }
        }
    }
}
