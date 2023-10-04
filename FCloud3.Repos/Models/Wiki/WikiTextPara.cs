using FCloud3.Repos.DB;
using FCloud3.Repos.Models.Corr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiTextPara : IDbModel, IWikiPara
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
    }
    public class WikiTextParaMeta: IWikiPara
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

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
