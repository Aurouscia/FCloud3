using Aurouscia.TableEditor.Core.Excel;
using FCloud3.DbContexts;
using FCloud3.Entities.Table;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Table;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Etc;

namespace FCloud3.Services.Wiki
{
    public class WikiParaService(
        WikiParaRepo wikiParaRepo,
        FileItemRepo fileItemRepo,
        WikiItemRepo wikiItemRepo,
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo,
        LatestWikiExchangeService latestWikiExchangeService,
        IStorage storage) 
    {
        private readonly WikiParaRepo _wikiParaRepo = wikiParaRepo;
        private readonly FileItemRepo _fileItemRepo = fileItemRepo;
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly TextSectionRepo _textSectionRepo = textSectionRepo;
        private readonly FreeTableRepo _freeTableRepo = freeTableRepo;
        private readonly IStorage _storage = storage;

        public bool SetFileParaFileId(int paraId, int fileId, out string? errmsg)
        {
            var targetFileItem = _fileItemRepo.Existing.Where(x => x.Id == fileId).FirstOrDefault();
            if(targetFileItem is null)
            {
                errmsg = "找不到指定的文件";
                return false;
            }
            if(_wikiParaRepo.SetParaObjId(paraId, WikiParaType.File, fileId, out errmsg))
            {
                UpdateRelatedWiki(paraId);
                return true;
            }
            else
                return false;
        }
        public bool SetInfo(int paraId, string? nameOverride, out string? errmsg)
        {
            var success = _wikiParaRepo.SetInfo(paraId, nameOverride, out errmsg);
            if(success)
            {

                UpdateRelatedWiki(paraId);
                return true;
            }
            return false;
        }
        public WikiParaRawContentRes? GetParaRawContent(int paraId, out string? errmsg)
        {
            var p = _wikiParaRepo.All.FirstOrDefault(x => x.Id == paraId);
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
            else if (p.Type == WikiParaType.File)
            {
                var obj = _fileItemRepo.GetById(p.ObjectId);
                name = obj?.DisplayName;
                content = obj?.StorePathName;
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

        public bool ConvertXlsxToAuTable(int paraId, out string? errmsg)
        {
            var p = _wikiParaRepo.GetById(paraId);
            if (p is null)
            {
                errmsg = "找不到指定段落";
                return false;
            }
            if (p.Type != WikiParaType.File)
            {
                errmsg = "段落类型异常";
                return false;
            }
            var file = _fileItemRepo.GetById(p.ObjectId);
            if (file is null || file.StorePathName is null)
            {
                errmsg = "段落无文件";
                return false;
            }
            if (!file.StorePathName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                errmsg = "文件必须为xlsx格式";
                return false;
            }
            var stream = _storage.Read(file.StorePathName);
            if (stream is not null)
            {
                var tableData = AuTableExcelConverter.FromXlsx(
                    stream, CommonOptions.excelConvert, out errmsg);
                if (tableData is null || errmsg is { })
                {
                    errmsg ??= "文件解析失败";
                    return false;
                }
                string? name = p.NameOverride;
                if (string.IsNullOrWhiteSpace(name))
                    name = file.DisplayName;
                if (string.IsNullOrWhiteSpace(name))
                    name = "新导入表格";
                var createdTableId = _freeTableRepo.TryCreateWithContent(tableData, name, out errmsg);
                if (createdTableId > 0)
                {
                    p.Type = WikiParaType.Table;
                    p.ObjectId = createdTableId;
                    _wikiParaRepo.Update(p);
                    UpdateRelatedWiki(paraId);
                }
                else
                {
                    errmsg ??= "创建表格失败";
                    return false;
                }
                return true;
            }
            errmsg = "文件读取失败";
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
        private void UpdateRelatedWiki(int paraId)
        {
            int affectedWikiId = BelongToWikiId(paraId);
            if (affectedWikiId > 0)
            {
                _wikiItemRepo.UpdateTimeAndLuAndWikiActive(affectedWikiId, true);
                latestWikiExchangeService.Push();
            }
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
