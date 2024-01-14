using FCloud3.Entities;
using FCloud3.DbContexts;
using Microsoft.EntityFrameworkCore;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Entities.Wiki.Paragraph;

namespace FCloud3.Repos.TextSec
{
    public class TextSectionRepo : RepoBase<TextSection>
    {
        public TextSectionRepo(FCloudContext context) : base(context)
        {
        }

        public TextSection? GetByPara(WikiPara para)
        {
            if (para.Type != WikiParaType.Text)
                return null;
            return Existing.Where(x => x.Id == para.ObjectId).FirstOrDefault();
        }
        public List<TextSection> GetRangeByParas(List<WikiPara> paras)
        {
            var textSecIds = paras
                .Where(x => x.Type == WikiParaType.Text)
                .Select(x => x.ObjectId)
                .ToList();
            return Existing.Where(x => textSecIds.Contains(x.Id)).ToList();
        }

        public TextSectionMeta? GetMetaByPara(WikiPara para)
        {
            if (para.Type != WikiParaType.Text)
                return null;
            return Existing.Where(x => x.Id == para.ObjectId).GetMetaData().FirstOrDefault();
        }
        public List<TextSectionMeta> GetMetaRangeByParas(List<WikiPara> paras)
        {
            var textParaIds = paras
                .Where(x => x.Type == WikiParaType.Text)
                .Select(x => x.ObjectId)
                .ToList();
            return Existing.Where(x => textParaIds.Contains(x.Id)).GetMetaData().ToList();
        }

        public bool TryChangeTitle(int id, string newTitle, out string? errmsg)
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

        public bool TryChangeContent(int id, string newContent, out string? errmsg)
        {
            errmsg = null;
            string brief = TextSectionBrief(newContent);
            int changed = Existing.Where(s => s.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(x => x.Content, newContent)
                    .SetProperty(x=>x.ContentBrief, brief));

            if (changed == 1)
                return true;
            else
            {
                errmsg = "修改内容失败";
                return false;
            }
        }

        public static string TextSectionBrief(string content)
        {
            if (content.Length >= 30)
            {
                return string.Concat(content.AsSpan(0, 27), "...");
            }
            return content;
        }
    }

    /// <summary>
    /// 文本段的元数据，只查询元数据可避免加载段落的所有文本，降低数据库负担
    /// </summary>
    public class TextSectionMeta : IWikiParaObject
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public WikiParaDisplay ToDisplay(WikiPara para)
        {
            return new WikiParaDisplay(para, Id, Title, ContentBrief, WikiParaType.Text);
        }

        public WikiParaDisplay ToDisplaySimple(WikiPara para)
        {
            return new WikiParaDisplay(para, Id, Title, ContentBrief, WikiParaType.Text);
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
