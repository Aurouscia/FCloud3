using Aurouscia.TableEditor.Core;
using Aurouscia.TableEditor.Core.Html;
using FCloud3.Entities.Files;
using FCloud3.Entities.Table;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using FCloud3.HtmlGen.Context.SubContext;
using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.Repos.Files;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.WikiParsing;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.WikiParsing.Support;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace FCloud3.Services.WikiParsing
{
    public class WikiParsingService
    {
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly WikiParaRepo _wikiParaRepo;
        private readonly TextSectionRepo _textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly WikiParserProviderService _wikiParserProvider;
        private readonly IStorage _storage;
        private readonly IMemoryCache _cache;

        public WikiParsingService(
            WikiItemRepo wikiItemRepo,
            WikiParaRepo wikiParaRepo,
            TextSectionRepo textSectionRepo,
            FreeTableRepo freeTableRepo,
            FileItemRepo fileItemRepo,
            WikiParserProviderService wikiParserProvider,
            IStorage storage,
            IMemoryCache cache)
        {
            _wikiItemRepo = wikiItemRepo;
            _wikiParaRepo = wikiParaRepo;
            _textSectionRepo = textSectionRepo;
            _freeTableRepo = freeTableRepo;
            _fileItemRepo = fileItemRepo;
            _wikiParserProvider = wikiParserProvider;
            _storage = storage;
            _cache = cache;
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

            var parser = _wikiParserProvider.Get($"w_{wiki.Id}");

            WikiParsingResult result = new()
            {
                Title = wiki.Title ?? "??"
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
                    result.AddRules(resOfP.UsedRules.ConvertAll(x => x.UniqueName));
                    result.FootNotes.AddRange(resOfP.FootNotes);
                    result.Paras.Add(new(model.Title, titleId, resOfP.Content, p.Type));
                }
                else if (p.Type == WikiParaType.Table)
                {
                    FreeTable? model = tableParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    var resOfP = ParseTable(model, parser);
                    parser.WrapSection(model.Name, resOfP.Titles, out var title, out int titleId);
                    result.SubTitles.Add(title);
                    result.AddRules(resOfP.UsedRules.ConvertAll(x => x.UniqueName));
                    result.FootNotes.AddRange(resOfP.FootNotes);
                    result.Paras.Add(new(model.Name, titleId, resOfP.Content, p.Type));
                }
                else if (p.Type == WikiParaType.File)
                {
                    FileItem? model = fileParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    result.Paras.Add(new(model.DisplayName, 0, _storage.FullUrl(model.StorePathName ?? "??"), p.Type));
                }
            });
            return result;
        }
        public static ParserResultRaw ParseText(TextSection model, Parser parser)
        {
            return parser.RunToParserResultRaw(model.Content);
        }
        public static ParserResultRaw ParseTable(FreeTable model, Parser parser)
        {
            AuTable data = model.GetData();
            List<IRule> usedRules = new();
            var html = data.ConvertToHtml(new()
            {
                CellConverter = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return "　";
                    var res = parser.RunToParserResultRaw(s, false);
                    usedRules.AddRange(res.UsedRules);
                    return res.Content;
                }
            });
            return new(html, usedRules);
        }

        public class WikiParsingResult
        {
            /// <summary>
            /// 该wiki使用的“有共同部分(脚本/样式)”的规则，前端应该使用这个数组在本地缓存或api拿取脚本和样式
            /// </summary>
            public string Title { get; set; }
            public List<string> UsedRules { get; set; }
            public List<string> FootNotes { get; set; }
            public List<ParserTitleTreeNode> SubTitles { get; set; }
            public List<WikiParsingResultItem> Paras { get; set; }
            public WikiParsingResult()
            {
                Title = "";
                UsedRules = new();
                FootNotes = new();
                SubTitles = new();
                Paras = new();
            }
            public void AddRules(List<string> rules)
            {
                UsedRules = UsedRules.Union(rules).ToList();
            }
            public class WikiParsingResultItem
            {
                public string? Title { get; set; }
                public int TitleId { get; set; }
                public string? Content { get; set; }
                public WikiParaType ParaType { get; set; }
                public WikiParsingResultItem(string? title,int titleId, string content, WikiParaType type)
                {
                    Title = title;
                    TitleId = titleId;
                    Content = content;
                    ParaType = type;
                }
            }
            public static WikiParsingResult FallToInstance
            {
                get {
                    return new WikiParsingResult()
                    {
                        Paras = new()
                        {
                            new("找不到指定路径名的词条", 0, "可能是词条不存在或已被移走，请确认后重试", WikiParaType.Text)
                        }
                    };
                }
            }
        }
    }
}
