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
using FCloud3.Services.Etc;
using FCloud3.Services.Wiki;

namespace FCloud3.Services.Table
{
    public class FreeTableService(
        FreeTableRepo freeTableRepo,
        WikiParaRepo wikiParaRepo,
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        FileDirRepo fileDirRepo,
        WikiTitleContainService wikiTitleContainService,
        DiffContentService diffContentService,
        ContentEditLockService contentEditLockService,
        DbTransactionService dbTransactionService,
        LatestWikiExchangeService latestWikiExchangeService,
        ILogger<FreeTableService> logger)
    {
        public FreeTable? GetForEditing(int id, out string? errmsg)
        {
            if (!contentEditLockService.Heartbeat(HeartbeatObjType.FreeTable, id, true, out errmsg))
                return null;
            var freeTable = freeTableRepo.GetById(id);
            if (freeTable is null)
            {
                errmsg = "找不到指定表格";
                return null;
            }
            return freeTable;
        }

        public FreeTableMeta? GetMeta(int id)
        {
            return freeTableRepo.GetqById(id).GetMeta().FirstOrDefault();
        }
        
        public bool TryEditInfo(int id, string name, out string? errmsg)
        {
            if (freeTableRepo.TryEditInfo(id, name, out errmsg))
            {
                var affectedWikiIds = wikiParaRepo.WikiContainingIt(WikiParaType.Table, id);
                wikiItemRepo.UpdateTime(affectedWikiIds);
                return true;
            }
            else
                return false;
        }
        public bool TryEditContent(int id, string data, out string? errmsg)
        {
            if (!contentEditLockService.Heartbeat(HeartbeatObjType.FreeTable, id, false, out errmsg))
                return false;
            var model = freeTableRepo.GetById(id);
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
            using var transaction = dbTransactionService.BeginTransaction();
            var diffCharCount = diffContentService.MakeDiff(id, DiffContentType.FreeTable, oldData, data, out string? diffErrmsg);
            var diffSuccess = diffErrmsg is null;
            var saveSuccess = freeTableRepo.TryEditContent(model, datatable, data, out string? saveErrmsg);

            if (saveSuccess && diffSuccess)
            {
                dbTransactionService.CommitTransaction(transaction);
                logger.LogInformation("更新[{id}]号表格成功", id);

                var affectedWikiIds = wikiParaRepo.WikiContainingIt(WikiParaType.Table, id).ToList();
                var affectedCount = wikiItemRepo.UpdateTimeAndLuAndWikiActive(affectedWikiIds, true);
                if (affectedCount > 0)
                {
                    var containingWikiDirs = wikiToDirRepo.GetDirIdsByWikiIds(affectedWikiIds).ToList();
                    fileDirRepo.SetUpdateTimeRangeAncestrally(containingWikiDirs, out _);
                }
                wikiTitleContainService.AutoAppendForOne(
                    WikiTitleContainType.FreeTable, id, affectedWikiIds ?? [], data);
                latestWikiExchangeService.Push();
                return true;
            }
            else
            {
                dbTransactionService.RollbackTransaction(transaction);
                errmsg = saveErrmsg + diffErrmsg;
                logger.LogError("更新[{id}]号表格失败，\"{msg}\"", id, errmsg);
                return false;
            }
        }

        public int TryAddAndAttach(int paraId, out string? errmsg)
        {
            int createdTableId = freeTableRepo.AddDefaultAndGetId();
            if (createdTableId <= 0)
            {
                errmsg = "未知错误，表格创建失败";
                return 0;
            }
            if (!wikiParaRepo.SetParaObjId(paraId, WikiParaType.Table, createdTableId, out errmsg))
                return 0;
            return createdTableId;
        }
    }
}
