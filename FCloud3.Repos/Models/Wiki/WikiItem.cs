using FCloud3.Repos.DB;
using FCloud3.Repos.Models.Corr;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiItem : IDbModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int OwnerUserId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public class WikiItemRepo : RepoBase<WikiItem>
    {
        private readonly CorrRepo _corrRepo;
        private readonly WikiTextParaRepo _textParaRepo;
        private readonly List<CorrType> wikiParaTypes;
        public WikiItemRepo(FCloudContext context, CorrRepo corrRepo, WikiTextParaRepo textParaRepo) : base(context)
        {
            _corrRepo = corrRepo;
            _textParaRepo = textParaRepo;
            wikiParaTypes = WikiParaTypeUtil.WikiParaCorrTypes;
        }

        public List<Corr.Corr> GetWikiParaCorrs(int wikiId, int start = 0, int count = int.MaxValue)
        {
            var corrs = _context.Corrs
                .Where(x => x.B == wikiId)
                .Where(x => wikiParaTypes.Contains(x.CorrType))
                .OrderBy(x => x.Order)
                .Skip(start)
                .Take(count)
                .ToList();
            return corrs;
        }

        public List<WikiParaDisplay> GetWikiParaDisplays(int wikiId,int start = 0, int count = int.MaxValue)
        {
            var corrs = GetWikiParaCorrs(wikiId, start, count);
            EnsureParaOrderDense(corrs);

            List<WikiTextParaMeta> textParas = _textParaRepo.GetMetaRangeByParaCorr(corrs);

            List<KeyValuePair<Corr.Corr ,IWikiPara>> paras = corrs.ConvertAll(x =>
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
                return new KeyValuePair<Corr.Corr, IWikiPara>(x,para);
            });

            List<WikiParaDisplay> res = paras.ToDisplaySimpleList();
            return res;
        }
        public bool InsertPara(int wikiId,int afterOrder, WikiParaType type, out string? errmsg)
        {
            var itsParas = GetWikiParaCorrs(wikiId);
            EnsureParaOrderDense(itsParas);
            var moveBackwards = itsParas.FindAll(x=>x.Order > afterOrder);
            moveBackwards.ForEach(x => x.Order++);
            Corr.Corr newPara = new()
            {
                B = wikiId,
                CorrType = type.ToCorrType(),
                Order = afterOrder + 1,
            };
            if (!_corrRepo.TryAdd(newPara, out errmsg))
                return false;
            itsParas.Add(newPara);
            EnsureParaOrderDense(itsParas);
            if (!_corrRepo.TryEditRange(itsParas, out errmsg))
                return false;
            return true;
        }

        public bool SetParaOrders(int wikiId,List<int> OrderedParaIds, out string? errmsg)
        {
            var itsParas = GetWikiParaCorrs(wikiId);
            EnsureParaOrderDense(itsParas);

            List<Corr.Corr> orderedParaCorrs = new(itsParas.Count); 
            foreach(int id in OrderedParaIds)
            {
                var p = itsParas.Find(x=>x.Id==id) ?? throw new Exception("在重设顺序时找不到指定id段落");
                orderedParaCorrs.Add(p);
            }
            ResetOrder(orderedParaCorrs);
            if(!_corrRepo.TryEditRange(orderedParaCorrs, out errmsg))
                return false;
            return true;
        }

        private static void EnsureParaOrderDense(List<Corr.Corr> corrs)
        {
            corrs.Sort((x, y) => x.Order - y.Order);
            ResetOrder(corrs);
        }
        private static void ResetOrder(List<Corr.Corr> corrs)
        {
            for (int i = 0; i < corrs.Count; i++)
                corrs[i].Order = i;
        }
    }
}
