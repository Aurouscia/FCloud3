using FCloud3.Repos.DB;
using FCloud3.Repos.Models.Corr;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiTextPara : IDbModel, IWikiPara, ICorrable
    {
        public int Id { get;set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get;set; }
        public DateTime Created { get;set; }
        public DateTime Updated { get;set; }
        public bool Deleted { get;set; }

        public WikiParaDisplay ToDisplay()
        {
            return new WikiParaDisplay()
            {
                Id = this.Id,
                Content = this.Content,
                Title = this.Title,
                Type = WikiParaType.Text
            };
        }
        public WikiParaDisplay ToDisplaySimple()
        {
            return new WikiParaDisplay()
            {
                Id = this.Id,
                Content = this.ContentBrief,
                Title = this.Title,
                Type = WikiParaType.Text
            };
        }
        public bool MatchedCorr(Corr.Corr corr)
        {
            if(corr.CorrType == Corr.CorrType.WikiTextPara_WikiItem)
            {
                if (corr.A == this.Id)
                    return true;
            }
            return false;
        }
    }

    public class WikiTextParaRepo : RepoBase<WikiTextPara>
    {
        public WikiTextParaRepo(FCloudContext context) : base(context)
        {
        }

        public WikiTextPara? GetByParaCorr(Corr.Corr paraCorr)
        {
            if (paraCorr.CorrType.ToWikiPara() != WikiParaType.Text)
                return null;
            return Existing.Where(x => x.Id == paraCorr.A).FirstOrDefault();
        }
        public List<WikiTextPara> GetRangeByParaCorr(List<Corr.Corr> paraCorrs)
        {
            var textParaIds = paraCorrs
                .Where(x => x.CorrType.ToWikiPara() == WikiParaType.Text)
                .Select(x => x.A)
                .ToList();
            return Existing.Where(x => textParaIds.Contains(x.Id)).ToList();
        }

        public WikiTextParaMeta? GetMetaByParaCorr(Corr.Corr paraCorr)
        {
            if (paraCorr.CorrType.ToWikiPara() != WikiParaType.Text)
                return null;
            return Existing.Where(x => x.Id == paraCorr.A).GetMetaData().FirstOrDefault();
        }
        public List<WikiTextParaMeta> GetMetaRangeByParaCorr(List<Corr.Corr> paraCorrs)
        {
            var textParaIds = paraCorrs
                .Where(x => x.CorrType.ToWikiPara() == WikiParaType.Text)
                .Select(x => x.A)
                .ToList();
            return Existing.Where(x => textParaIds.Contains(x.Id)).GetMetaData().ToList();
        }
    }

    public class WikiTextParaMeta: IWikiPara, ICorrable
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public bool MatchedCorr(Corr.Corr corr)
        {
            if (corr.CorrType == Corr.CorrType.WikiTextPara_WikiItem)
            {
                if (corr.A == this.Id)
                    return true;
            }
            return false;
        }

        public WikiParaDisplay ToDisplay()
        {
            throw new NotImplementedException();
        }

        public WikiParaDisplay ToDisplaySimple()
        {
            return new WikiParaDisplay()
            {
                Id = this.Id,
                Content = this.ContentBrief,
                Title = this.Title,
                Type = WikiParaType.Text
            };
        }
    }
    public static class WikiTextParaMetaQuerier
    {
        public static IQueryable<WikiTextParaMeta> GetMetaData(this IQueryable<WikiTextPara> source)
        {
            return source.Select(x => new WikiTextParaMeta()
            {
                Id = x.Id,
                Title = x.Title,
                ContentBrief = x.ContentBrief,
                CreatorUserId = x.CreatorUserId,
                Created = x.Created,
                Updated = x.Updated,
                Deleted = x.Deleted,
            });
        }
    }
}
