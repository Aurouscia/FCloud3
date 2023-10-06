using FCloud3.Repos.Models.Cor;
using FCloud3.Repos.Models.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Wiki
{
    public class WikiItemService
    {
        private readonly WikiItemRepo _wikiRepo;
        public const int maxWikiTitleLength = 30;
        public WikiItemService(WikiItemRepo wikiRepo)
        {
            _wikiRepo = wikiRepo;
        }
        public static bool BasicInfoCheck(WikiItem w, out string? errmsg)
        {
            errmsg = null;
            if (string.IsNullOrEmpty(w.Title))
            {
                errmsg = "标题不能为空";
                return false;
            }
            if (w.Title.Length > maxWikiTitleLength)
            {
                errmsg = $"标题不能超过{maxWikiTitleLength}字";
                return false;
            }
            return true;
        }

        public WikiItem? GetById(int id)
        {
            return _wikiRepo.GetById(id);
        }

        public List<WikiParaDisplay> GetWikiParaDisplays(int wikiId,int count=int.MaxValue)
        {
            return _wikiRepo.GetWikiParaDisplays(wikiId, 0, count);
        } 

        public List<WikiParaDisplay>? InsertPara(int wikiId, int afterOrder, WikiParaType type, out string? errmsg)
        {
            if (!_wikiRepo.InsertPara(wikiId, afterOrder, type, out errmsg))
                return null;
            return GetWikiParaDisplays(wikiId);
        }

        public List<WikiParaDisplay>? SetParaOrders(int wikiId, List<int> orderedParaIds,out string? errmsg)
        {
            if (!_wikiRepo.SetParaOrders(wikiId, orderedParaIds, out errmsg))
                return null;
            return GetWikiParaDisplays(wikiId);
        }

        public bool TryAdd(int creator,string? title, out string? errmsg)
        {
            WikiItem w = new()
            {
                CreatorUserId = creator,
                OwnerUserId = creator,
                Title = title
            };
            if (!BasicInfoCheck(w, out errmsg))
                return false;
            if (!_wikiRepo.TryAdd(w,out errmsg))
                return false;
            return true;
        }
        public bool TryEdit(int id, string? title, out string? errmsg)
        {
            WikiItem w = _wikiRepo.GetById(id) ?? throw new Exception("找不到指定id的wiki");
            w.Title = title;
            if (!BasicInfoCheck(w, out errmsg))
                return false;
            if (!_wikiRepo.TryEdit(w, out errmsg))
                return false;
            return true;
        }

    }
}
