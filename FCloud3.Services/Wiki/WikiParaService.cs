using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.Etc.Caching;

namespace FCloud3.Services.Wiki
{
    public class WikiParaService
    {
        private readonly WikiParaRepo _wikiParaRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly WikiItemCaching _wikiItemCaching;

        public WikiParaService(
            WikiParaRepo wikiParaRepo,
            FileItemRepo fileItemRepo,
            WikiItemRepo wikiItemRepo,
            WikiItemCaching wikiItemCaching) 
        {
            _wikiParaRepo = wikiParaRepo;
            _fileItemRepo = fileItemRepo;
            _wikiItemRepo = wikiItemRepo;
            _wikiItemCaching = wikiItemCaching;
        }

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
                {
                    _wikiItemRepo.SetUpdateTime(affectedWikiId);
                    _wikiItemCaching.Update(affectedWikiId, w => w.Update = DateTime.Now);
                }
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
                if (affectedWikiId > 0)
                {
                    _wikiItemRepo.SetUpdateTime(affectedWikiId);
                    _wikiItemCaching.Update(affectedWikiId, w => w.Update = DateTime.Now);
                }
                return true;
            }
            return false;
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
    }
}
