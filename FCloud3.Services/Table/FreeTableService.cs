using FCloud3.Entities.Table;
using FCloud3.Repos.Table;
using FCloud3.Repos.Wiki;
using FCloud3.Entities.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Table
{
    public class FreeTableService
    {
        private readonly FreeTableRepo _freeTableRepo;
        private readonly WikiParaRepo _wikiParaRepo;

        public FreeTableService(FreeTableRepo freeTableRepo, WikiParaRepo wikiParaRepo)
        {
            _freeTableRepo = freeTableRepo;
            _wikiParaRepo = wikiParaRepo;
        }

        public FreeTable? GetById(int id)
        {
            return _freeTableRepo.GetById(id);
        }
        public bool TryEditInfo(int id, string name, out string? errmsg)
        {
            return _freeTableRepo.TryEditInfo(id, name, out errmsg);
        }
        public bool TryEditContent(int id, string data, out string? errmsg)
        {
            return _freeTableRepo.TryEditContent(id, data, out errmsg);
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
