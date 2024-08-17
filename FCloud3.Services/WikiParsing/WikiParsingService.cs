using Aurouscia.TableEditor.Core;
using Aurouscia.TableEditor.Core.Excel;
using Aurouscia.TableEditor.Core.Html;
using DotNetColorParser;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Table;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using FCloud3.WikiPreprocessor.Context.SubContext;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.Repos.Files;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.WikiParsing.Support;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.Identities;
using FCloud3.Services.Identities;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Services.WikiParsing
{
    public class WikiParsingService(
        WikiItemRepo wikiItemRepo,
        WikiItemCaching wikiItemCaching,
        WikiParaRepo wikiParaRepo,
        WikiTitleContainRepo wikiTitleContainRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo,
        FileItemRepo fileItemRepo,
        UserRepo userRepo,
        UserGroupRepo userGroupRepo,
        UserToGroupRepo userToGroupRepo,
        MaterialCaching materialCaching,
        WikiParserProviderService wikiParserProvider,
        WikiParsedResultService wikiParsedResult,
        AuthGrantService authGrantService,
        IStorage storage,
        IOperatingUserIdProvider userIdProvider,
        ILogger<WikiParsingService> logger,
        IConfiguration config)
    {
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiItemCaching _wikiItemCaching = wikiItemCaching;
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly WikiTitleContainRepo _wikiTitleContainRepo = wikiTitleContainRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;
        private readonly FileItemRepo _fileItemRepo = fileItemRepo;
        private readonly UserRepo _userRepo = userRepo;
        private readonly UserGroupRepo _userGroupRepo = userGroupRepo;
        private readonly UserToGroupRepo _userToGroupRepo = userToGroupRepo;
        private readonly MaterialCaching _materialCaching = materialCaching;
        private readonly WikiParserProviderService _wikiParserProvider = wikiParserProvider;
        private readonly WikiParsedResultService _wikiParsedResult = wikiParsedResult;
        private readonly AuthGrantService _authGrantService = authGrantService;
        private readonly IStorage _storage = storage;
        private readonly IOperatingUserIdProvider _userIdProvider = userIdProvider;
        private readonly ILogger<WikiParsingService> _logger = logger;
        private readonly bool debug = config["Debug"] == "on";

        public WikiDisplayInfo? GetWikiDisplayInfo(string pathName, bool defaultAccess = false)
        {
            var info = (
                from w in _wikiItemRepo.Existing
                from u in _userRepo.All
                where w.UrlPathName == pathName
                where w.OwnerUserId == u.Id
                select new
                {
                    WikiId = w.Id,
                    UserId = u.Id,
                    UserName = u.Name,
                    UserAvtId = u.AvatarMaterialId,
                    WikiSealed = w.Sealed
                }).FirstOrDefault();
            if(info is null)
                return null;

            var access = defaultAccess;
            if(!access) 
                access = _authGrantService.Test(AuthGrantOn.WikiItem, info.WikiId);
            
            var groupLabels = (
                from ug in _userGroupRepo.Existing
                from utg in _userToGroupRepo.ExistingAndShowLabel
                where utg.UserId == info.UserId
                where utg.GroupId == ug.Id
                select new { ug.Id, ug.Name, ug.OwnerUserId }
                ).ToList();
            var uid = _userIdProvider.Get();
            groupLabels.Sort((x, y) =>
            {
                int xOwned = x.OwnerUserId == uid ? 1 : 0;
                int yOwned = y.OwnerUserId == uid ? 1 : 0;
                if (xOwned != yOwned)
                    return yOwned - xOwned;
                return string.Compare(x.Name, y.Name, StringComparison.InvariantCulture);
            });
            string? avtSrc = null;
            if (info.UserAvtId > 0)
            {
                avtSrc = _materialCaching.Get(info.UserAvtId)?.PathName;
                if (avtSrc is not null)
                {
                    avtSrc = _storage.FullUrl(avtSrc);
                }
            }
            var resp = new WikiDisplayInfo(
                info.WikiId, info.UserName, avtSrc, info.WikiSealed, access);
            groupLabels.ForEach(l =>
            {
                resp.UserGroupLabels.Add(new(l.Id, l.Name));
            });
            return resp;
        }
        
        public Stream? GetParsedWikiStream(string pathName, bool bypassSeal = false)
        {
            var w = _wikiItemCaching.Get(pathName);
            if (w is null)
                return null;
            if (w.Sealed && _userIdProvider.Get() != w.OwnerId && !bypassSeal)
                return null;//对于隐藏的词条，又不是拥有者又不是管理，就当不存在的
            return GetParsedWikiStream(w.Id, w.Update);
        }
        public Stream GetParsedWikiStream(int id, DateTime update)
        {
            lock (GetLockObj(id))
            {
                Stream? stream = null;
                if(!debug)
                    stream = _wikiParsedResult.Read(id, update);
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

            var contains = textContains.Union(tableContains).ToList();
            var allWikis = _wikiItemCaching.GetAll().FindAll(x => !x.Sealed);
            var parser = _wikiParserProvider.Get($"w_{wiki.Id}", 
                allWikis,
                configure: builder => builder.Cache.DisableCache(),//片段缓存必须关闭
                contains,
                true,
                () => [wiki.Id]);
            parser.Context.Reset(true);

            WikiParsingResult result = new(wiki.Id, wiki.Title??"??", wiki.Updated, wiki.OwnerUserId);
            string? getTitle(string? nameoverride, string? title, bool parse = true)
            {
                string? t = nameoverride;
                if (string.IsNullOrWhiteSpace(t))
                    t = title;
                if (!parse)
                    return t;
                var resOft = parser.RunToParserResultRaw(t,false);
                result.AddRules(resOft.UsedRules);
                return resOft.Content;
            }
            paras.ForEach(p =>
            {
                //更换为该段的自动替换目标
                var containType = WikiTitleContainType.Unknown;
                if (p.Type == WikiParaType.Text)
                    containType = WikiTitleContainType.TextSection;
                else if (p.Type == WikiParaType.Table)
                    containType = WikiTitleContainType.FreeTable;
                var itsContains = contains.AsQueryable()
                    .WithTypeAndId(containType, p.ObjectId).Select(x=>x.WikiId).ToList();
                var itsContainDetects = itsContains.ConvertAll(wid =>
                    allWikis.Find(w => w.Id == wid)?.Title);
                parser.Context.AutoReplace.Register(itsContainDetects);
                
                if (p.Type == WikiParaType.Text)
                {
                    TextSection? model = textParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    var resOfP = ParseText(model, parser);
                    var realTitle = getTitle(p.NameOverride, model.Title);
                    parser.WrapSection(realTitle, resOfP.Titles, out var title, out int titleId);
                    result.SubTitles.Add(title);
                    result.AddRules(resOfP.UsedRulesWithCommons);
                    result.FootNotes.AddRange(resOfP.FootNotes);
                    result.Paras.Add(new(
                        realTitle, titleId, resOfP.Content, p.Id, p.Type,
                        p.ObjectId, 0, true, true));
                }
                else if (p.Type == WikiParaType.Table)
                {
                    FreeTable? model = tableParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    var resOfP = ParseTable(model, parser);
                    var realTitle = getTitle(p.NameOverride, model.Name);
                    parser.WrapSection(realTitle, resOfP.Titles, out var title, out int titleId);
                    result.SubTitles.Add(title);
                    result.AddRules(resOfP.UsedRulesWithCommons);
                    result.FootNotes.AddRange(resOfP.FootNotes);
                    result.Paras.Add(new(
                        realTitle, titleId, resOfP.Content, p.Id, p.Type,
                        p.ObjectId, 0, true, true));
                }
                else if (p.Type == WikiParaType.File)
                {
                    FileItem? model = fileParaObjs.FirstOrDefault(x => x.Id == p.ObjectId);
                    if (model is null)
                        return;
                    if (model.StorePathName is not null &&
                        model.StorePathName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = _storage.Read(model.StorePathName);
                        string? errmsg = "解析失败";
                        if (stream is not null)
                        {
                            var tableData = AuTableExcelConverter.FromXlsx(stream, out errmsg);
                            if (tableData is not null)
                            {
                                var resOfP = ParseTable(tableData, parser);
                                var realTitle = getTitle(p.NameOverride, model.DisplayName);
                                parser.WrapSection(realTitle, resOfP.Titles, out var title, out int titleId);
                                result.SubTitles.Add(title);
                                result.AddRules(resOfP.UsedRulesWithCommons);
                                result.FootNotes.AddRange(resOfP.FootNotes);
                                WikiParsingResult.WikiParsingResultItem para = new(
                                    realTitle, titleId, resOfP.Content, p.Id, WikiParaType.Table,
                                    p.ObjectId, 0, false, false);
                                para.IsFromFile = true;
                                result.Paras.Add(para);
                                return;
                            }
                        }
                        errmsg ??= "xlsx文件可能格式异常";
                        WikiParsingResult.WikiParsingResultItem errPara = new(
                            "xlsx表格解析失败", 0, errmsg, p.Id, WikiParaType.Text, 
                            p.ObjectId, 0, false, false);
                        errPara.IsFromFile = true;
                        result.Paras.Add(errPara);
                    }
                    else
                    {
                        var realTitle = getTitle(p.NameOverride, model.DisplayName, false);
                        result.Paras.Add(new(
                            realTitle, 0, _storage.FullUrl(model.StorePathName ?? "??"), p.Id, p.Type,
                            p.ObjectId, model.ByteCount, false, false));
                    }
                }
            });
            result.ExtractRulesCommon();
            return result;
        }
        private static ParserResultRaw ParseText(TextSection model, Parser parser)
        {
            return parser.RunToParserResultRaw(model.Content);
        }
        private ParserResultRaw ParseTable(AuTable data, Parser parser)
        {
            var colorParser = parser.Context.Options.ColorParser;
            List<IRule> usedRules = new();
            Func<string?, (string tdContent, string tdAttrs)> cellConverter;
            if (data.Cells is not null && data.Cells.ConvertAll(x => x?.Count).Sum() <= 2000)
                cellConverter = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return ("　", "");
                    var colorRes = MiniTableBlockRule.CellColorAttr(s, colorParser);
                    var res = parser.RunToParserResultRaw(colorRes.s, false);
                    usedRules.AddRange(res.UsedRules);
                    return (res.Content, colorRes.attrs);
                };
            else
                cellConverter = x => (x ?? "　", "");
            var html = data.ConvertToHtml(new()
            {
                CellConverterAttr = cellConverter
            });
            return new(html, usedRules);
        }
        private ParserResultRaw ParseTable(FreeTable model, Parser parser)
        {
            AuTable data = model.GetData();
            return ParseTable(data, parser);
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
            public int Id { get; set; }
            public string Title { get; set; }
            public string Update { get; set; }
            public int OwnerId { get; set; }
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
                Id = 0;
                Title = "";
                Update = "";
                OwnerId = 0;
                UsedRules = [];
                FootNotes = [];
                SubTitles = [];
                Paras = [];
                UsedRulesBody = [];
            }
            public WikiParsingResult(int id, string title, DateTime update, int ownerId)
            {
                Id = id;
                Title = title;
                Update = update.ToString("yyyy-MM-dd HH:mm");
                OwnerId = ownerId;
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
                var allStyles = UsedRulesBody.ConvertAll(x => x.GetStyles());
                allStyles.RemoveAll(string.IsNullOrWhiteSpace);
                Styles = string.Join("\n\n", allStyles);

                var allPreScripts = UsedRulesBody.ConvertAll(x => x.GetPreScripts());
                allPreScripts.RemoveAll(string.IsNullOrWhiteSpace);
                PreScripts = string.Join("\n\n", allPreScripts);

                var allPostScripts = UsedRulesBody.ConvertAll(x => x.GetPostScripts());
                allPostScripts.RemoveAll(string.IsNullOrWhiteSpace);
                PostScripts = string.Join("\n\n", allPostScripts);
            }
            public class WikiParsingResultItem
            {
                public string? Title { get; set; }
                public int TitleId { get; set; }
                public string? Content { get; set; }
                public int ParaId { get; set; }
                public WikiParaType ParaType { get; set; }
                public bool IsFromFile { get; set; }
                public int UnderlyingId { get; set; }
                public int Bytes { get; set; }
                public bool Editable { get; set; }
                public bool HistoryViewable { get; set; }
                public WikiParsingResultItem(
                    string? title,int titleId, string content, int paraId, WikiParaType type,
                    int underlyingId, int bytes, bool editable, bool historyViewable)
                {
                    Title = title;
                    TitleId = titleId;
                    Content = content;
                    ParaType = type;
                    ParaId = paraId;
                    UnderlyingId = underlyingId;
                    Bytes = bytes;
                    Editable = editable;
                    HistoryViewable = historyViewable;
                }
            }
            public static WikiParsingResult FallToInstance
            {
                get {
                    return new WikiParsingResult()
                    {
                        Paras = new()
                        {
                            new("找不到指定路径名的词条", 0, "可能是词条不存在或已被移走，请确认后重试", 0, WikiParaType.Text, 0, 0, false, false)
                        }
                    };
                }
            }
        }
        public class WikiDisplayInfo(
            int wikiId, string userName, string? userAvtSrc,
            bool @sealed, bool currentUserAccess)
        {
            public int WikiId { get; set; } = wikiId;
            public string UserName { get; set; } = userName;
            public string? UserAvtSrc { get; set; } = userAvtSrc;
            public bool Sealed { get; set; } = @sealed;
            public bool CurrentUserAccess { get; set; } = currentUserAccess;
            public List<UserGroupLabel> UserGroupLabels { get; set; } = [];
            public struct UserGroupLabel(int id, string name)
            {
                public int Id { get; set; } = id;
                public string Name { get; set; } = name;
            }
        }
    }
}
