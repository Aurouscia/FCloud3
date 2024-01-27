using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Wiki
{
    public class WikiParaService
    {
        private readonly WikiParaRepo _wikiParaRepo;
        private readonly FileItemRepo _fileItemRepo;

        public WikiParaService(WikiParaRepo wikiParaRepo, FileItemRepo fileItemRepo) 
        {
            _wikiParaRepo = wikiParaRepo;
            _fileItemRepo = fileItemRepo;
        }

        public bool SetFileParaFileId(int paraId, int fileId, out string? errmsg)
        {
            var targetFileItem = _fileItemRepo.Existing.Where(x => x.Id == fileId).FirstOrDefault();
            if(targetFileItem is null)
            {
                errmsg = "找不到指定的文件";
                return false;
            }
            return _wikiParaRepo.SetFileParaFileId(paraId, fileId, out errmsg);
        }
    }
}
