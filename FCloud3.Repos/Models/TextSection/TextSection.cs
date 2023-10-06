using FCloud3.Repos.DB;
using FCloud3.Repos.Models.Cor;
using FCloud3.Repos.Models.Wiki;

namespace FCloud3.Repos.Models.TextSec
{
    public class TextSection : IDbModel, IWikiPara, ICorrable
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public WikiParaDisplay ToDisplay(Corr corrWithCurrentWiki)
        {
            return new WikiParaDisplay(corrWithCurrentWiki,Title,Content,WikiParaType.Text);
        }
        public WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki)
        {
            return new WikiParaDisplay(corrWithCurrentWiki, Title, ContentBrief, WikiParaType.Text);
        }
        public bool MatchedCorr(Corr corr)
        {
            if (corr.CorrType == CorrType.TextSection_WikiItem)
            {
                if (corr.A == Id)
                    return true;
            }
            return false;
        }
    }

    public class TextSectionRepo : RepoBase<TextSection>
    {
        public TextSectionRepo(FCloudContext context) : base(context)
        {
        }

        public TextSection? GetByParaCorr(Corr paraCorr)
        {
            if (paraCorr.CorrType.ToWikiPara() != WikiParaType.Text)
                return null;
            return Existing.Where(x => x.Id == paraCorr.A).FirstOrDefault();
        }
        public List<TextSection> GetRangeByParaCorr(List<Corr> paraCorrs)
        {
            var textParaIds = paraCorrs
                .Where(x => x.CorrType.ToWikiPara() == WikiParaType.Text)
                .Select(x => x.A)
                .ToList();
            return Existing.Where(x => textParaIds.Contains(x.Id)).ToList();
        }

        public TextSectionMeta? GetMetaByParaCorr(Corr paraCorr)
        {
            if (paraCorr.CorrType.ToWikiPara() != WikiParaType.Text)
                return null;
            return Existing.Where(x => x.Id == paraCorr.A).GetMetaData().FirstOrDefault();
        }
        public List<TextSectionMeta> GetMetaRangeByParaCorr(List<Corr> paraCorrs)
        {
            var textParaIds = paraCorrs
                .Where(x => x.CorrType.ToWikiPara() == WikiParaType.Text)
                .Select(x => x.A)
                .ToList();
            return Existing.Where(x => textParaIds.Contains(x.Id)).GetMetaData().ToList();
        }
    }

    public class TextSectionMeta : IWikiPara, ICorrable
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public bool MatchedCorr(Corr corr)
        {
            if (corr.CorrType == CorrType.TextSection_WikiItem)
            {
                if (corr.A == Id)
                    return true;
            }
            return false;
        }

        public WikiParaDisplay ToDisplay(Corr corrWithCurrentWiki)
        {
            return new WikiParaDisplay(corrWithCurrentWiki, Title, ContentBrief, WikiParaType.Text);
        }

        public WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki)
        {
            return new WikiParaDisplay(corrWithCurrentWiki, Title, ContentBrief, WikiParaType.Text);
        }
    }
    public static class TextSectionParaMetaQuerier
    {
        public static IQueryable<TextSectionMeta> GetMetaData(this IQueryable<TextSection> source)
        {
            return source.Select(x => new TextSectionMeta()
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
