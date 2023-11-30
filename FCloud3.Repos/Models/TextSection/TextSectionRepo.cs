using FCloud3.Entities.DbModels;
using FCloud3.Entities.DbModels.Corr;
using FCloud3.Entities.DbModels.TextSec;
using FCloud3.Repos.DbContexts;
using FCloud3.Repos.Models.Cor;
using FCloud3.Repos.Models.Wiki;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos.Models.TextSec
{
    //public class TextSection : IDbModel, IWikiPara, ICorrable
    //{
    //    public int Id { get; set; }
    //    public string? Title { get; set; }
    //    public string? Content { get; set; }
    //    public string? ContentBrief { get; set; }

    //    public int CreatorUserId { get; set; }
    //    public DateTime Created { get; set; }
    //    public DateTime Updated { get; set; }
    //    public bool Deleted { get; set; }

    //    public WikiParaDisplay ToDisplay(Corr corrWithCurrentWiki)
    //    {
    //        return new WikiParaDisplay(corrWithCurrentWiki, Id,Title,Content,WikiParaType.Text);
    //    }
    //    public WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki)
    //    {
    //        return new WikiParaDisplay(corrWithCurrentWiki, Id,Title, ContentBrief, WikiParaType.Text);
    //    }
    //    public bool MatchedCorr(Corr corr)
    //    {
    //        if (corr.CorrType == CorrType.TextSection_WikiItem)
    //        {
    //            if (corr.A == Id)
    //                return true;
    //        }
    //        return false;
    //    }
    //    public static string? Brief(string? content)
    //    {
    //        if (content is not null)
    //        {
    //            if(content.Length>20)
    //                return content[..20];
    //            return content;
    //        }
    //        return null;
    //    }
    //}

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

        public bool TryChangeTitle(int id,string newTitle,out string? errmsg)
        {
            errmsg = null;
            int changed = Existing.Where(s => s.Id == id).ExecuteUpdate(s => s.SetProperty(x => x.Title, newTitle));
            if (changed == 1)
                return true;
            else
            {
                errmsg = "修改标题失败";
                return false;
            }
        }

        public bool TryChangeContent(int id,string newContent,out string? errmsg)
        {
            errmsg = null;
            int changed = Existing.Where(s => s.Id == id).ExecuteUpdate(s => s.SetProperty(x => x.Content, newContent));

            throw new NotImplementedException();
            //string? brief = TextSection.Brief(newContent);
            //Existing.Where(s => s.Id == id).ExecuteUpdate(s => s.SetProperty(x => x.ContentBrief, brief));

            //if (changed == 1)
            //    return true;
            //else
            //{
            //    errmsg = "修改内容失败";
            //    return false;
            //}
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
            return new WikiParaDisplay(corrWithCurrentWiki, Id, Title, ContentBrief, WikiParaType.Text);
        }

        public WikiParaDisplay ToDisplaySimple(Corr corrWithCurrentWiki)
        {
            return new WikiParaDisplay(corrWithCurrentWiki, Id, Title, ContentBrief, WikiParaType.Text);
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
