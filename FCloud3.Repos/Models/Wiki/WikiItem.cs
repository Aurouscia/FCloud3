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
        private readonly List<CorrType> wikiParaTypes;
        public WikiItemRepo(FCloudContext context, CorrRepo corrRepo) : base(context)
        {
            _corrRepo = corrRepo;
            wikiParaTypes = new List<CorrType>
            { 
                CorrType.WikiTextPara_WikiItem,
                CorrType.WikiFilePara_WikiItem,
                CorrType.WikiTablePara_WikiItem
            };
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
            EnsureParaOrder(corrs);

            var textParaIds = corrs
                .Where(x => x.CorrType.ToWikiPara() == WikiParaType.Text)
                .Select(x=>x.A).ToList();
            List<WikiTextParaMeta> textParas = _context.WikiTextParas
                .Where(x =>textParaIds.Contains(x.Id))
                .GetMetaData()
                .ToList();

            List<IWikiPara> paras = corrs.ConvertAll(x =>
            {
                WikiParaType type = x.CorrType.ToWikiPara();
                IWikiPara? para;
                if (type == WikiParaType.Text)
                {
                    para = textParas.Find(p => p.Id == x.A);
                }
                else
                {
                    throw new NotImplementedException();
                }
                para ??= new WikiParaPlaceholder(type);
                return para;
            });

            List<WikiParaDisplay> res = paras.ToDisplaySimpleList(corrs);
            return res;
        }
        public bool InsertPara(int wikiId,int afterOrder, WikiParaType type, out string? errmsg)
        {
            var itsParas = GetWikiParaCorrs(wikiId);
            EnsureParaOrder(itsParas);
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
            EnsureParaOrder(itsParas);
            if (!_corrRepo.TryEditRange(itsParas, out errmsg))
                return false;
            return true;
        }

        public bool SetParaOrders(int wikiId,List<int> OrderedParaIds, out string? errmsg)
        {
            var itsParas = GetWikiParaCorrs(wikiId);
            EnsureParaOrder(itsParas);

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

        private static void EnsureParaOrder(List<Corr.Corr> corrs)
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
