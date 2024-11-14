using FCloud3.Entities.TextSection;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.TextSec;
using FCloud3.Entities.Wiki;
using FCloud3.DbContexts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FCloud3.Services.Diff;
using FCloud3.Entities.Diff;
using FCloud3.Services.Etc.TempData.EditLock;
using FCloud3.Repos.Files;
using FCloud3.Services.WikiParsing.Support;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Util;
using FCloud3.Services.Etc;

namespace FCloud3.Services.TextSec
{
    public class TextSectionService(
        WikiItemRepo wikiItemRepo,
        WikiParaRepo wikiParaRepo,
        TextSectionRepo textSectionRepo,
        WikiToDirRepo wikiToDirRepo,
        WikiTitleContainRepo wikiTitleContainRepo,
        FileDirRepo fileDirRepo,
        DiffContentService contentDiffService,
        DbTransactionService dbTransactionService,
        ContentEditLockService contentEditLockService,
        WikiParserProviderService wikiParserProviderService,
        WikiParserDataSource wikiParserDataSource,
        LatestWikiExchangeService latestWikiExchangeService,
        ILocatorHash locatorHash, 
        ILogger<TextSectionService> logger,
        IConfiguration config)
    {
        private readonly bool debug = config["Debug"] == "on";
        public TextSection? GetForEditing(int id, out string? errmsg)
        {
            if (!contentEditLockService.Heartbeat(HeartbeatObjType.TextSection, id, true, out errmsg))
                return null;
            var textSection = textSectionRepo.GetById(id);
            if (textSection is null)
            {
                errmsg = "找不到指定文本段落";
                return null;
            }
            return textSection;
        }

        public TextSectionMeta? GetMeta(int id)
        {
            return textSectionRepo.GetqById(id).GetMeta().FirstOrDefault();
        }
        
        /// <summary>
        /// 新建一个文本段
        /// </summary>
        /// <returns>新建的文本段Id</returns>
        public int AddDefaultAndGetId() => textSectionRepo.AddDefaultAndGetId();

        /// <summary>
        /// 新建一个文本段并关联指定段落
        /// </summary>
        /// <returns>新建的文本段Id</returns>
        public int TryAddAndAttach(int paraId, out string? errmsg)
        {
            int createdTextId = AddDefaultAndGetId();
            if (createdTextId <= 0)
            {
                errmsg = "未知错误，文本创建失败";
                return 0;
            }
            if (!wikiParaRepo.SetParaObjId(paraId, WikiParaType.Text, createdTextId, out errmsg))
                return 0;
            return createdTextId;
        }
        /// <summary>
        /// 更新一个文本段，可只更新标题或只更新内容，另一者设为null即可
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public bool TryUpdate(int id, string? title, string? content, out string? errmsg)
        {
            if (id == 0)
            {
                errmsg = "未得到更新文本段Id";
                return false;
            }
            if (title is not null)
            {
                if (!textSectionRepo.TryChangeTitle(id, title, out errmsg))
                    return false;
            }
            int diffCharCount = 0;
            if (content is not null)
            {
                if (!contentEditLockService.Heartbeat(HeartbeatObjType.TextSection, id, false, out errmsg))
                    return false;
                var original = textSectionRepo.GetById(id);
                if (original is null)
                {
                    errmsg = "找不到指定文本段";
                    return false;
                }
                if (original.Content == content)
                {
                    errmsg = null;
                    return true;
                }
                using var t = dbTransactionService.BeginTransaction();
                var updateSuccess = textSectionRepo.TryChangeContent(id, content, out var updateErrmsg);
                diffCharCount = contentDiffService.MakeDiff(id, DiffContentType.TextSection, original.Content, content, out var diffErrmsg);
                var diffSuccess = diffErrmsg is null;
                if (updateSuccess && diffSuccess)
                {
                    dbTransactionService.CommitTransaction(t);
                    logger.LogInformation("更新[{id}]号文本段成功", id);
                }
                else
                {
                    dbTransactionService.RollbackTransaction(t);
                    errmsg = updateErrmsg + diffErrmsg;
                    logger.LogError("更新[{id}]号文本段失败，\"{msg}\"", id, errmsg);
                    return false;
                }
            }

            if(title is not null || content is not null)
            {
                var affectedWikiIds = wikiParaRepo.WikiContainingIt(WikiParaType.Text, id);
                var affectedCount = wikiItemRepo.UpdateTimeAndLuAndWikiActive(affectedWikiIds, true);
                if (affectedCount > 0)
                {
                    var containingWikiDirs = wikiToDirRepo.GetDirIdsByWikiIds(affectedWikiIds).ToList();
                    fileDirRepo.SetUpdateTimeRangeAncestrally(containingWikiDirs, out _);
                    latestWikiExchangeService.Push();
                }
            }
            errmsg = null;
            return true;
        }

        public TextSectionPreviewResponse Preview(int id, string content)
        {
            string cacheKey = $"tse_{id}";
            var parser = wikiParserProviderService.Get(
                cacheKey,
                true,
                builder => {
                    builder.UseLocatorHash(locatorHash);
                    builder.Block.SetTitleLevelOffset(1);
                    if (debug)
                        builder.EnableDebugInfo();
                },
                () => wikiTitleContainRepo.GetByTypeAndObjId(
                        WikiTitleContainType.TextSection, id),
                true,
                () => wikiParaRepo.WikiContainingIt(WikiParaType.Text, id).ToArray()
            );
            parser.SetDataSource(wikiParserDataSource);
            var res = new TextSectionPreviewResponse(parser.RunToParserResult(content));
            return res;
        }
        public class TextSectionPreviewResponse
        {
            public string HtmlSource { get; }
            public string PreScripts { get; }
            public string PostScripts { get; }
            public string Styles { get; }
            public TextSectionPreviewResponse(string htmlSource)
            {
                HtmlSource = htmlSource;
                PreScripts = "";
                PostScripts = "";
                Styles = "";
            }
            public TextSectionPreviewResponse(ParserResult parserResult)
            {
                HtmlSource = parserResult.Content + parserResult.FootNotes;
                PreScripts = parserResult.PreScript;
                PostScripts = parserResult.PostScript;
                Styles = parserResult.Style;
            }
        }
    }
}
