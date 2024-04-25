using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Services.Wiki
{
    public class WikiParaService
    {
        private readonly WikiParaRepo _wikiParaRepo;
        private readonly FileItemRepo _fileItemRepo;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly CacheExpTokenService _cacheExpTokenService;

        public WikiParaService(
            WikiParaRepo wikiParaRepo,
            FileItemRepo fileItemRepo,
            WikiItemRepo wikiItemRepo,
            CacheExpTokenService cacheExpTokenService) 
        {
            _wikiParaRepo = wikiParaRepo;
            _fileItemRepo = fileItemRepo;
            _wikiItemRepo = wikiItemRepo;
            _cacheExpTokenService = cacheExpTokenService;
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
                var q =
                    from w in _wikiItemRepo.Existing
                    from p in _wikiParaRepo.Existing
                    where p.Id == paraId
                    where p.WikiItemId == w.Id
                    select w;
                var affected = q.ExecuteUpdate(x => x.SetProperty(w => w.Updated, DateTime.Now));
                if(affected > 0)
                    _cacheExpTokenService.WikiItemInfo.CancelAll();
                return true;
            }
            else
                return false;
        }
    }
}
