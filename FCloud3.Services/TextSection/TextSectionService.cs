using FCloud3.Entities.TextSection;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.TextSec;
using FCloud3.Entities.Wiki;
using FCloud3.DbContexts;
using Microsoft.Extensions.Logging;
using FCloud3.Services.Diff;
using FCloud3.Entities.Diff;
using FCloud3.Services.Etc.TempData.EditLock;

namespace FCloud3.Services.TextSec
{
    public class TextSectionService
    {
        private readonly WikiParaRepo _paraRepo;
        private readonly TextSectionRepo _textSectionRepo;
        private readonly int _userId;
        private readonly DiffContentService _contentDiffService;
        private readonly DbTransactionService _dbTransactionService;
        private readonly ContentEditLockService _contentEditLockService;
        private readonly ILogger<TextSectionService> _logger;

        public TextSectionService(
            IOperatingUserIdProvider userIdProvider,
            WikiParaRepo paraRepo,
            TextSectionRepo textsectionRepo,
            DiffContentService contentDiffService,
            DbTransactionService dbTransactionService,
            ContentEditLockService contentEditLockService,
            ILogger<TextSectionService> logger)
        {
            _paraRepo = paraRepo;
            _textSectionRepo = textsectionRepo;
            _userId = userIdProvider.Get();
            _contentDiffService = contentDiffService;
            _dbTransactionService = dbTransactionService;
            _contentEditLockService = contentEditLockService;
            _logger = logger;
        }

        public TextSection? GetForEditing(int id, out string? errmsg)
        {
            if (!_contentEditLockService.Heartbeat(ObjectType.TextSection, id, out errmsg))
                return null;
            var textSection = _textSectionRepo.GetById(id);
            if (textSection is null)
            {
                errmsg = "找不到指定文本段落";
                return null;
            }
            return textSection;
        }

        public static bool ModelCheck(TextSection section, out string? errmsg)
        {
            errmsg = null;
            if (string.IsNullOrEmpty(section.Title))
            {
                errmsg = "标题不能为空";
                return false;
            }
            return true;
        }
        /// <summary>
        /// 新建一个文本段
        /// </summary>
        /// <returns>新建的文本段Id</returns>
        public int TryAdd(out string? errmsg)
        {
            TextSection newSection = new()
            {
                Title = "新建文本段",
                Content = "",
                ContentBrief = "",
                CreatorUserId = _userId
            };
            if (!ModelCheck(newSection, out errmsg))
                return 0;
            if (!_textSectionRepo.TryAdd(newSection, out errmsg))
                return 0;
            return newSection.Id;
        }
        /// <summary>
        /// 新建一个文本段并关联指定段落
        /// </summary>
        /// <returns>新建的文本段Id</returns>
        public int TryAddAndAttach(int paraId, out string? errmsg)
        {
            var para = _paraRepo.GetById(paraId) ?? throw new Exception("找不到指定Id的段落");
            if (para.Type != WikiParaType.Text)
            {
                errmsg = "段落类型检查出错";
                return 0;
            }
            int createdTextId = TryAdd(out errmsg);
            if (createdTextId <= 0)
                return 0;
            para.ObjectId = createdTextId;
            if (!_paraRepo.TryEdit(para, out errmsg))
                return 0;
            return createdTextId;
        }
        /// <summary>
        /// 更新一个文本段
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
            if (!_contentEditLockService.Heartbeat(ObjectType.TextSection, id, out errmsg))
                return false;
            if (title is not null)
            {
                if (!_textSectionRepo.TryChangeTitle(id, title, out errmsg))
                    return false;
            }
            if (content is not null)
            {
                var original = _textSectionRepo.GetById(id);
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
                using var t = _dbTransactionService.BeginTransaction();
                var updateSuccess = _textSectionRepo.TryChangeContent(id, content, out var updateErrmsg);
                var diffSuccess = _contentDiffService.MakeDiff(id, DiffContentType.TextSection, original.Content, content, out var diffErrmsg);
                if (updateSuccess && diffSuccess)
                {
                    _dbTransactionService.CommitTransaction(t);
                    _logger.LogInformation("更新[{id}]号文本段成功", id);
                }
                else
                {
                    _dbTransactionService.RollbackTransaction(t);
                    errmsg = updateErrmsg + diffErrmsg;
                    _logger.LogError("更新[{id}]号文本段失败，\"{msg}\"", id, errmsg);
                    return false;
                }
            }
            errmsg = null;
            return true;
        }
    }
}
