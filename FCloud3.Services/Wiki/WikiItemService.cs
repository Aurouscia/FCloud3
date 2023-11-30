using FCloud3.Entities.Corr;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Cor;
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
        private readonly WikiItemRepo _wikiRepo;
        private readonly CorrRepo _corrRepo;
        private readonly TextSectionRepo _textSectionRepo;
        private readonly List<CorrType> _wikiParaTypes;
        public const int maxWikiTitleLength = 30;
        public WikiItemService(WikiItemRepo wikiRepo,CorrRepo corrRepo,TextSectionRepo textSectionRepo)
        {
            _wikiRepo = wikiRepo;
            _corrRepo = corrRepo;
            _textSectionRepo = textSectionRepo;
            _wikiParaTypes = WikiParaTypeUtil.WikiParaCorrTypes;
        }
        public WikiItem? GetById(int id)
        {
            return _wikiRepo.GetById(id);
        }
        public List<Corr> GetWikiParaCorrs(int wikiId, int start = 0, int count = int.MaxValue)
        {
            var corrs = _corrRepo.Existing
                .Where(x => x.B == wikiId)
                .Where(x => _wikiParaTypes.Contains(x.CorrType))
                .OrderBy(x => x.Order)
                .Skip(start)
                .Take(count)
                .ToList();
            return corrs;
        }
        public bool BuildParaCorr(int wikiId, WikiParaType type, int order, out string? errmsg)
        {
            Corr c = new()
            {
                B = wikiId,
                Order = order,
                CorrType = type.ToCorrType()
            };
            return _corrRepo.TryAdd(c, out errmsg);
        }
        public List<WikiParaDisplay> GetWikiParaDisplays(int wikiId, int start = 0, int count = int.MaxValue)
        {
            if (!_wikiRepo.Existing.Any(x => x.Id == wikiId))
            {
                throw new Exception("找不到指定id的wiki");
            }
            var corrs = GetWikiParaCorrs(wikiId, start, count);
            EnsureParaOrderDense(corrs);

            List<TextSectionMeta> textParas = _textSectionRepo.GetMetaRangeByParaCorr(corrs);

            List<KeyValuePair<Corr, IWikiPara>> paras = corrs.ConvertAll(x =>
            {
                WikiParaType type = x.CorrType.ToWikiPara();
                IWikiPara? para = null;
                if (type == WikiParaType.Text)
                {
                    para = textParas.Find(p => p.MatchedCorr(x));
                }
                else
                {
                    //throw new NotImplementedException();
                }
                para ??= new WikiParaPlaceholder(type);
                return new KeyValuePair<Corr, IWikiPara>(x, para);
            });

            List<WikiParaDisplay> res = paras.ToDisplaySimpleList();
            return res;
        }
        public bool InsertPara(int wikiId, int afterOrder, WikiParaType type, out string? errmsg)
        {
            _wikiRepo.BeginTransaction();
            var itsParas = GetWikiParaCorrs(wikiId);
            EnsureParaOrderDense(itsParas);
            var moveBackwards = itsParas.FindAll(x => x.Order > afterOrder);
            moveBackwards.ForEach(x => x.Order++);
            if (!BuildParaCorr(wikiId, type, afterOrder + 1,out errmsg))
                return false;
            EnsureParaOrderDense(itsParas);
            if (!_corrRepo.TryEditRange(itsParas, out errmsg))
                return false;
            _wikiRepo.CommitTransaction();

            return true;
        }
        public bool SetParaOrders(int wikiId, List<int> orderedParaIds, out string? errmsg)
        {
            var itsParas = GetWikiParaCorrs(wikiId);
            EnsureParaOrderDense(itsParas);
            if (orderedParaIds.Count < itsParas.Count)
            {
                errmsg = "数据不一致，请刷新页面后重试";
                return false;
            }
            List<Corr> orderedParaCorrs = new(itsParas.Count);
            foreach (int id in orderedParaIds)
            {
                var p = itsParas.Find(x => x.Id == id) ?? throw new Exception("在重设顺序时找不到指定id段落");
                orderedParaCorrs.Add(p);
            }
            ResetOrder(orderedParaCorrs);
            if (!_corrRepo.TryEditRange(orderedParaCorrs, out errmsg))
                return false;
            return true;
        }
        private static void EnsureParaOrderDense(List<Corr> corrs)
        {
            corrs.Sort((x, y) => x.Order - y.Order);
            ResetOrder(corrs);
        }
        private static void ResetOrder(List<Corr> corrs)
        {
            for (int i = 0; i < corrs.Count; i++)
                corrs[i].Order = i;
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
        public bool TryRemovePara(int id, int corrId, out string? errmsg)
        {
            var paras = GetWikiParaCorrs(id);
            var target = paras.Find(x => x.Id == corrId);
            if(target is null)
            {
                errmsg = "未找到指定Id的目标段落";
                return false;
            }
            paras.Remove(target);
            _corrRepo.BeginTransaction();
            EnsureParaOrderDense(paras);
            if (!_corrRepo.TryEditRange(paras, out errmsg))
            {
                _corrRepo.RollbackTransaction();
                return false;
            }
            if (!_corrRepo.TryRemove(target, out errmsg))
            {
                _corrRepo.RollbackTransaction();
                return false;
            }
            _corrRepo.CommitTransaction();
            return true;
        }
    }
}
