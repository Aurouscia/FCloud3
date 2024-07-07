using FCloud3.Entities.Wiki;
using FCloud3.Services.Etc.TempData.EditLock;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class HeartbeatController(
        ContentEditLockService contentEditLockService,
        WikiItemService wikiItemService): Controller
    {
        private readonly ContentEditLockService _contentEditLockService = contentEditLockService;
        private readonly WikiItemService _wikiItemService = wikiItemService;

        public IActionResult Do(HeartbeatObjType objType, int objId)
        {
            if (!_contentEditLockService.Heartbeat(objType, objId, false, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult DoRangeForWiki(int wikiId)
        {
            var paras = _wikiItemService.GetWikiParas(wikiId);
            
            List<(HeartbeatObjType type, int objId)> heartBeats = [];
            foreach (var p in paras)
            {
                if (p.Type == WikiParaType.Text)
                    heartBeats.Add((HeartbeatObjType.TextSection, p.ObjectId));
                else if (p.Type == WikiParaType.Table)
                    heartBeats.Add((HeartbeatObjType.FreeTable, p.ObjectId));
            }
            if (!_contentEditLockService.HeartbeatRange(heartBeats, false, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        
        public IActionResult Release(HeartbeatObjType objType, int objId)
        {
            _contentEditLockService.ReleaseLock(objType, objId);
            return this.ApiResp();
        }
        
        public IActionResult ReleaseRangeForWiki(int wikiId)
        {
            var paras = _wikiItemService.GetWikiParas(wikiId);
            
            List<(HeartbeatObjType type, int objId)> heartBeats = [];
            foreach (var p in paras)
            {
                if (p.Type == WikiParaType.Text)
                    heartBeats.Add((HeartbeatObjType.TextSection, p.ObjectId));
                else if (p.Type == WikiParaType.Table)
                    heartBeats.Add((HeartbeatObjType.FreeTable, p.ObjectId));
            }
            _contentEditLockService.ReleaseLockRange(heartBeats);
            return this.ApiResp();
        }
    }
}
