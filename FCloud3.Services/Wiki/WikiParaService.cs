using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Table;

namespace FCloud3.Services.Wiki
{
    public class WikiParaService(
        WikiParaRepo wikiParaRepo,
        FileItemRepo fileItemRepo,
        WikiItemRepo wikiItemRepo,
        WikiItemCaching wikiItemCaching,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo) 
    {
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly FileItemRepo _fileItemRepo = fileItemRepo;
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly WikiItemCaching _wikiItemCaching = wikiItemCaching;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;

        public bool SetFileParaFileId(int paraId, int fileId, out string? errmsg)
        {
            var targetFileItem = _fileItemRepo.Existing.Where(x => x.Id == fileId).FirstOrDefault();
            if(targetFileItem is null)
            {
                errmsg = "找不到指定的文件";
                return false;
            }
            if(_wikiParaRepo.SetFileParaFileId(paraId, fileId, out errmsg))
            {
                int affectedWikiId = BelongToWikiId(paraId);
                if(affectedWikiId > 0)
                    _wikiItemRepo.UpdateTime(affectedWikiId);
                return true;
            }
            else
                return false;
        }
        public IQueryable<int> WikiContainingIt(WikiParaType type, int objId) => _wikiParaRepo.WikiContainingIt(type, objId);
        public bool SetInfo(int paraId, string? nameOverride, out string? errmsg)
        {
            var success = _wikiParaRepo.SetInfo(paraId, nameOverride, out errmsg);
            if(success)
            {
                int affectedWikiId = BelongToWikiId(paraId);
                if(affectedWikiId > 0)
                    _wikiItemRepo.UpdateTime(affectedWikiId);
                return true;
            }
            return false;
        }
        public WikiParaRawContentRes? GetParaRawContent(int paraId, out string? errmsg)
        {
            var p = _wikiParaRepo.GetById(paraId);
            if(p is null)
            {
                errmsg = "找不到指定段落";
                return null;
            }
            var wOwner = _wikiItemRepo.GetqById(p.WikiItemId)
                .Select(x => x.OwnerUserId).FirstOrDefault();
            string ? content = null;
            string? name = null;
            if(p.Type == WikiParaType.Text)
            {
                var obj = _textSectionRepo.GetById(p.ObjectId);
                name = obj?.Title;
                content = obj?.Content;
            }
            else if(p.Type == WikiParaType.Table)
            {
                var obj = _freeTableRepo.GetById(p.ObjectId);
                name = obj?.Name;
                content = obj?.Data;
            }
            else
            {
                errmsg = "非可查看原内容段落类型";
                return null;
            }
            if(!string.IsNullOrWhiteSpace(p.NameOverride))
                name = p.NameOverride;
            errmsg = null;
            return new() {
                ParaId = paraId,
                ParaName = name,
                ParaType = p.Type,
                ObjId = p.ObjectId,
                OwnerId = wOwner,
                Content = content,
                LastEdit = p.Updated.ToString("yyyy/MM/dd HH:mm")
            };
        }

        private int BelongToWikiId(int paraId)
        {
            var q =
                from w in _wikiItemRepo.Existing
                from p in _wikiParaRepo.Existing
                where p.Id == paraId
                where p.WikiItemId == w.Id
                select w;
            return q.Select(x => x.Id).FirstOrDefault();
        }

        public class WikiParaRawContentRes
        {
            public int ParaId { get; set; }
            public string? ParaName { get; set; }
            public WikiParaType ParaType { get; set; }
            public int ObjId { get; set; }
            public int OwnerId { get; set; }
            public string? LastEdit { get; set; }
            public string? Content { get; set; }
        }
    }
}
