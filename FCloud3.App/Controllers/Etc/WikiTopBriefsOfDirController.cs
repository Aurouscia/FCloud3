using FCloud3.Entities.Wiki;
using FCloud3.Repos.Identities;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class WikiTopBriefsOfDirController(
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        WikiParaRepo wikiParaRepo,
        UserRepo userRepo,
        TextSectionRepo textSectionRepo
        ): Controller
    {
        public IActionResult Get(int dirId, int count)
        {
            var newestWsQ =
                from wd in wikiToDirRepo.Existing
                from w in wikiItemRepo.Existing
                from u in userRepo.Existing
                where wd.DirId == dirId
                where wd.WikiId == w.Id
                where w.OwnerUserId == u.Id
                orderby w.LastActive descending
                select new { 
                    w.Id, w.UrlPathName,
                    w.Title, w.LastActive,
                    OwnerName = u.Name };
            var newestWs = newestWsQ.Take(count).ToList();
            var newestWIds = newestWs.ConvertAll(x => x.Id);
            var relatedTextParas = (
                from wp in wikiParaRepo.Existing
                where newestWIds.Contains(wp.WikiItemId)
                where wp.Type == WikiParaType.Text
                select wp).ToList();
            var firstTexts = new List<(int wikiId, int textId)>();
            foreach(var w in newestWs)
            {
                var firstText = relatedTextParas
                    .Where(x => x.WikiItemId == w.Id)
                    .OrderBy(x => x.Order)
                    .Select(x => new
                    {
                        x.WikiItemId,
                        x.ObjectId
                    })
                    .FirstOrDefault();
                if(firstText is { })
                    firstTexts.Add((firstText.WikiItemId, firstText.ObjectId));
            }
            var textIds = firstTexts.ConvertAll(x => x.textId);
            var textBriefs = (
                from ts in textSectionRepo.Existing
                where textIds.Contains(ts.Id)
                select new
                {
                    ts.Id,
                    ts.ContentBrief
                }).ToList();
            var resp = new WikiTopBriefOfDirResponse();
            foreach (var w in newestWs)
            {
                var firstTextId = firstTexts.Find(x => x.wikiId == w.Id).textId;
                var firstTextBrief = textBriefs.Find(t => t.Id == firstTextId)?.ContentBrief;
                if(firstTextBrief is { })
                    resp.Items.Add(new()
                    {
                        Id = w.Id,
                        PathName = w.UrlPathName,
                        OwnerName = w.OwnerName,
                        Brief = firstTextBrief,
                        Time = w.LastActive.ToString("MM/dd"),
                        Title = w.Title
                    });
            }
            return this.ApiResp(resp);
        }
        public class WikiTopBriefOfDirResponse
        {
            public List<WikiTopBriefOfDirItem> Items { get; set; } = [];
            public class WikiTopBriefOfDirItem
            {
                public int Id { get; set; }
                public string? PathName { get; set; }
                public string? Title { get; set; }
                public string? Time { get; set; }
                public string? OwnerName { get; set; }
                public string? Brief { get; set; }
            }
        }
    }
}
