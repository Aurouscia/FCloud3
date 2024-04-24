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

namespace FCloud3.Services.Table
{
    public class FreeTableService
    {
        private readonly FreeTableRepo _freeTableRepo;
        private readonly WikiParaRepo _wikiParaRepo;
        private readonly DiffContentService _diffContentService;
        private readonly ContentEditLockService _contentEditLockService;
        private readonly DbTransactionService _dbTransactionService;
        private readonly ILogger<FreeTableService> _logger;

        public FreeTableService(
            FreeTableRepo freeTableRepo,
            WikiParaRepo wikiParaRepo,
            DiffContentService diffContentService,
            ContentEditLockService contentEditLockService,
            DbTransactionService dbTransactionService,
            ILogger<FreeTableService> logger)
        {
            _freeTableRepo = freeTableRepo;
            _wikiParaRepo = wikiParaRepo;
            _diffContentService = diffContentService;
            _contentEditLockService = contentEditLockService;
            _dbTransactionService = dbTransactionService;
            _logger = logger;
        }

        public FreeTable? GetForEditing(int id, out string? errmsg)
        {
            if (!_contentEditLockService.Heartbeat(HeartbeatObjType.FreeTable, id, out errmsg))
                return null;
            var freeTable = _freeTableRepo.GetById(id);
            if (freeTable is null)
            {
                errmsg = "找不到指定表格";
                return null;
            }
            return freeTable;
        }
        public bool TryEditInfo(int id, string name, out string? errmsg)
        {
            if (!_contentEditLockService.Heartbeat(HeartbeatObjType.FreeTable, id, out errmsg))
                return false;
            return _freeTableRepo.TryEditInfo(id, name, out errmsg);
        }
        public bool TryEditContent(int id, string data, out string? errmsg)
        {
            if (!_contentEditLockService.Heartbeat(HeartbeatObjType.FreeTable, id, out errmsg))
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
                errmsg = null;
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
            var para = _wikiParaRepo.GetById(paraId) ?? throw new Exception("找不到指定Id的段落");
            if(para.Type!=WikiParaType.Table)
            {
                errmsg = "段落类型检查出错";
                return 0;
            }
            int createdTableId = _freeTableRepo.TryCreateDefaultAndGetId(out errmsg);
            if (createdTableId <= 0)
                return 0;
            para.ObjectId = createdTableId;
            if (!_wikiParaRepo.TryEdit(para, out errmsg))
                return 0;
            return createdTableId;
        }
    }
}
