using FCloud3.Entities;
using FCloud3.DbContexts;
using Microsoft.EntityFrameworkCore;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.TextSec
{
    public class TextSectionRepo : RepoBase<TextSection>
    {
        public TextSectionRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public TextSectionMeta? GetMetaById(int id)
        {
            return Existing.Where(x => x.Id == id).GetMeta().FirstOrDefault();
        }
        public List<TextSectionMeta> GetMetaRangeByIds(List<int> ids)
        {
            if (ids.Count == 0)
                return new();
            return base.GetRangeByIds(ids).GetMeta().ToList();
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
    public class TextSectionMeta
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public static class TextSectionMetaQuerier
    {
        public static IQueryable<TextSectionMeta> GetMeta(this IQueryable<TextSection> source)
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
