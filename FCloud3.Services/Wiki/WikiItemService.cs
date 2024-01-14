using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Wiki;
using FCloud3.Entities.Wiki.Paragraph;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Wiki
{
    public class WikiItemService
    {
        private readonly DbTransactionService _transaction;
        private readonly WikiItemRepo _wikiRepo;
        private readonly WikiParaRepo _paraRepo;
        private readonly TextSectionRepo _textSectionRepo;
        private readonly List<WikiParaType> _wikiParaTypes;
        public const int maxWikiTitleLength = 30;
        public WikiItemService(
            DbTransactionService transaction,
            WikiItemRepo wikiRepo,WikiParaRepo paraRepo,
            TextSectionRepo textSectionRepo)
        {
            _transaction = transaction;
            _wikiRepo = wikiRepo;
            _paraRepo = paraRepo;
            _textSectionRepo = textSectionRepo;
            _wikiParaTypes = WikiParaTypes.GetListInstance();
        }
        public WikiItem? GetById(int id)
        {
            return _wikiRepo.GetById(id);
        }
        /// <summary>
        /// 获取某wiki的一部分或全部段落
        /// </summary>
        /// <param name="wikiId">wiki的Id</param>
        /// <param name="start">从第几段开始（0是第一段）</param>
        /// <param name="count">取几段</param>
        /// <returns></returns>
        public List<WikiPara> GetWikiParas(int wikiId, int start = 0, int count = int.MaxValue)
        {
            var paras = _paraRepo.Existing
                .Where(x => x.WikiItemId == wikiId)
                .OrderBy(x => x.Order)
                .Skip(start)
                .Take(count)
                .ToList();
            return paras;
        }
        /// <summary>
        /// 获取某wiki的一部分或全部段落，并转换为显示用的数据
        /// </summary>
        /// <param name="wikiId">wiki的Id</param>
        /// <param name="start">从第几段开始（0是第一段）</param>
        /// <param name="count">取几段</param>
        /// <returns></returns>
        public List<WikiParaDisplay> GetWikiParaDisplays(int wikiId, int start = 0, int count = int.MaxValue)
        {
            if (!_wikiRepo.Existing.Any(x => x.Id == wikiId))
            {
                throw new Exception("找不到指定id的wiki");
            }
            var paras = GetWikiParas(wikiId, start, count);
            paras.EnsureOrderDense();

            List<TextSectionMeta> textParas = _textSectionRepo.GetMetaRangeByParas(paras);

            List<KeyValuePair<WikiPara, IWikiParaObject>> paraObjs = paras.ConvertAll(x =>
            {
                WikiParaType type = x.Type;
                IWikiParaObject? para = null;
                if (type == WikiParaType.Text)
                {
                    para = textParas.Find(p => p.Id==x.ObjectId);
                }
                else
                {
                    //throw new NotImplementedException();
                }
                para ??= new WikiParaPlaceholder(type);
                return new KeyValuePair<WikiPara, IWikiParaObject>(x, para);
            });

            List<WikiParaDisplay> res = paraObjs.ToDisplaySimpleList();
            return res;
        }
        public bool InsertPara(int wikiId, int afterOrder, WikiParaType type, out string? errmsg)
        {
            string? msg = null;
            bool success = _transaction.DoTransaction(() =>
            {
                var itsParas = GetWikiParas(wikiId);
                itsParas.EnsureOrderDense();
                var moveBackwards = itsParas.FindAll(x => x.Order > afterOrder);
                moveBackwards.ForEach(x => x.Order++);
                if (!BuildPara(wikiId, type, afterOrder + 1, out msg))
                    return false;
                if (!_paraRepo.TryEditRange(itsParas, out msg))
                    return false;
                success = true;
                return true;
            });
            errmsg = msg;
            return success;
        }
        public bool SetParaOrders(int wikiId, List<int> orderedParaIds, out string? errmsg)
        {
            var itsParas = GetWikiParas(wikiId);
            itsParas.EnsureOrderDense();
            if (orderedParaIds.Count != itsParas.Count)
            {
                errmsg = "数据不一致，请刷新页面后重试";
                return false;
            }
            List<WikiPara> orderedParas = new(itsParas.Count);
            foreach (int id in orderedParaIds)
            {
                var p = itsParas.Find(x => x.Id == id);
                if (p is null)
                {
                    errmsg = "数据不一致，请刷新页面后重试";
                    return false;
                }
                orderedParas.Add(p);
            }
            orderedParas.ResetOrder();
            if (!_paraRepo.TryEditRange(orderedParas, out errmsg))
                return false;
            return true;
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
        public bool TryRemovePara(int id, int paraId, out string? errmsg)
        {
            var paras = GetWikiParas(id);
            var target = paras.Find(x => x.Id == paraId);
            if(target is null)
            {
                errmsg = "未找到指定Id的目标段落";
                return false;
            }
            paras.Remove(target);
            paras.EnsureOrderDense();
            string? msg = null;
            bool success = _transaction.DoTransaction(() =>
            {
                if (!_paraRepo.TryEditRange(paras, out msg))
                    return false;
                if (!_paraRepo.TryRemove(target, out msg))
                    return false;
                return true;
            });
            errmsg = msg;
            return success;
        }

        private bool BuildPara(int wikiId, WikiParaType type, int order, out string? errmsg)
        {
            WikiPara p = new()
            {
                WikiItemId = wikiId,
                Order = order,
                Type = type
            };
            return _paraRepo.TryAdd(p, out errmsg);
        }
    }
}
