using FCloud3.Entities.Table;
using FCloud3.Repos.Table;
using FCloud3.Repos.Wiki;
using FCloud3.Entities.Wiki;
using Aurouscia.TableEditor.Core.Utils;
using FCloud3.DbContexts;
using FCloud3.Services.Diff;
using FCloud3.Entities.Diff;
using Microsoft.Extensions.Logging;
using FCloud3.Services.Etc.TempData.EditLock;
using FCloud3.Repos.Files;

namespace FCloud3.Services.Table
{
    public class FreeTableService
    {
        private readonly FreeTableRepo _freeTableRepo;
        private readonly WikiParaRepo _wikiParaRepo;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly WikiToDirRepo _wikiToDirRepo;
        private readonly FileDirRepo _fileDirRepo;
        private readonly DiffContentService _diffContentService;
        private readonly ContentEditLockService _contentEditLockService;
        private readonly DbTransactionService _dbTransactionService;
        private readonly ILogger<FreeTableService> _logger;

        public FreeTableService(
            FreeTableRepo freeTableRepo,
            WikiParaRepo wikiParaRepo,
            WikiItemRepo wikiItemRepo,
            WikiToDirRepo wikiToDirRepo,
            FileDirRepo fileDirRepo,
            DiffContentService diffContentService,
            ContentEditLockService contentEditLockService,
            DbTransactionService dbTransactionService,
            ILogger<FreeTableService> logger)
        {
            _freeTableRepo = freeTableRepo;
            _wikiParaRepo = wikiParaRepo;
            _wikiItemRepo = wikiItemRepo;
            _wikiToDirRepo = wikiToDirRepo;
            _fileDirRepo = fileDirRepo;
            _diffContentService = diffContentService;
            _contentEditLockService = contentEditLockService;
            _dbTransactionService = dbTransactionService;
            _logger = logger;
        }

        public FreeTable? GetForEditing(int id, out string? errmsg)
        {
            if (!_contentEditLockService.Heartbeat(HeartbeatObjType.FreeTable, id, true, out errmsg))
                return null;
            var freeTable = _freeTableRepo.GetById(id);
            if (freeTable is null)
            {
                errmsg = "找不到指定表格";
                return null;
            }
            return freeTable;
        }

        public FreeTableMeta? GetMeta(int id)
        {
            return _freeTableRepo.GetqById(id).GetMeta().FirstOrDefault();
        }
        
        public bool TryEditInfo(int id, string name, out string? errmsg)
        {
            if (_freeTableRepo.TryEditInfo(id, name, out errmsg))
            {
                var affectedWikiIds = _wikiParaRepo.WikiContainingIt(WikiParaType.Table, id);
                _wikiItemRepo.UpdateTime(affectedWikiIds);
                return true;
            }
            else
                return false;
        }
        public bool TryEditContent(int id, string data, out string? errmsg)
        {
            if (!_contentEditLockService.Heartbeat(HeartbeatObjType.FreeTable, id, false, out errmsg))
                return false;
            var model = _freeTableRepo.GetById(id);
            if (model is null)
            {
                errmsg = "找不到指定表格";
                return false;
            }

            var datatable = FreeTableDataConvert.Deserialize(data);
            int rowCount = datatable.GetRowCount();
            int colCount = datatable.GetColumnCount();
            if (datatable.Cells is null || rowCount == 0 || colCount == 0)
            {
                errmsg = "不能保存空表格";
                return false;
            }

            var oldData = model.Data ?? "";
            using var transaction = _dbTransactionService.BeginTransaction();
            var diffSuccess = _diffContentService.MakeDiff(id, DiffContentType.FreeTable, oldData, data, out string? diffErrmsg);
            var saveSuccess = _freeTableRepo.TryEditContent(model, datatable, data, out string? saveErrmsg);

            if (saveSuccess && diffSuccess)
            {
                _dbTransactionService.CommitTransaction(transaction);
                _logger.LogInformation("更新[{id}]号表格成功", id);

                var affectedWikiIds = _wikiParaRepo.WikiContainingIt(WikiParaType.Table, id);
                var affectedCount = _wikiItemRepo.UpdateTime(affectedWikiIds);
                if (affectedCount > 0)
                {
                    var containingWikiDirs = _wikiToDirRepo.GetDirIdsByWikiIds(affectedWikiIds).ToList();
                    _fileDirRepo.SetUpdateTimeRangeAncestrally(containingWikiDirs, out _);
                }
                return true;
            }
            else
            {
                _dbTransactionService.RollbackTransaction(transaction);
                errmsg = saveErrmsg + diffErrmsg;
                _logger.LogError("更新[{id}]号表格失败，\"{msg}\"", id, errmsg);
                return false;
            }
        }

        public int TryAddAndAttach(int paraId, out string? errmsg)
        {
            int createdTableId = _freeTableRepo.AddDefaultAndGetId();
            if (createdTableId <= 0)
            {
                errmsg = "未知错误，表格创建失败";
                return 0;
            }
            if (!_wikiParaRepo.SetParaObjId(paraId, WikiParaType.Table, createdTableId, out errmsg))
                return 0;
            return createdTableId;
        }
    }
}
