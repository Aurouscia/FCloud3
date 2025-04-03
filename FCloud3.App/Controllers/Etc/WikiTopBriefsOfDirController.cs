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
        ) : Controller
    {
        public IActionResult Get([FromBody] WikiTopBriefOfDirRequest req)
        {
            var dirId = req.DirId;
            var skip = req.Skip;
            var take = req.Take;
            var briefAt = req.TakeBriefAt;
            var kvPairAt = req.TakeKvPairAt;
            if (briefAt < 0 && kvPairAt < 0)
                return this.ApiResp(new WikiTopBriefOfDirResponse());
            var newestWsQ =
                from wd in wikiToDirRepo.Existing
                from w in wikiItemRepo.Existing
                from u in userRepo.Existing
                where wd.DirId == dirId
                where wd.WikiId == w.Id
                where w.OwnerUserId == u.Id
                orderby w.LastActive descending
                select new
                {
                    w.Id,
                    w.UrlPathName,
                    w.Title,
                    w.LastActive,
                    OwnerName = u.Name
                };
            var newestWs = newestWsQ.Skip(skip).Take(take).ToList();
            var newestWIds = newestWs.ConvertAll(x => x.Id);
            var relatedTextParas = (
                from wp in wikiParaRepo.Existing
                where newestWIds.Contains(wp.WikiItemId)
                where wp.Type == WikiParaType.Text
                select wp).ToList();

            var briefParas = new List<(int wikiId, int textId)>();
            var kvPairParas = new List<(int wikiId, int textId)>();
            foreach (var w in newestWs)
            {
                var paraQ = relatedTextParas
                    .Where(x => x.WikiItemId == w.Id)
                    .OrderBy(x => x.Order)
                    .Select(x => new
                    {
                        x.WikiItemId,
                        x.ObjectId
                    });
                if (briefAt >= 0)
                {
                    var briefPara = paraQ
                        .Skip(briefAt).Take(1)
                        .FirstOrDefault();
                    if (briefPara is { })
                        briefParas.Add((briefPara.WikiItemId, briefPara.ObjectId));
                }
                if (kvPairAt >= 0)
                {
                    var kvPairPara = paraQ
                        .Skip(kvPairAt).Take(1)
                        .FirstOrDefault();
                    if (kvPairPara is { })
                        kvPairParas.Add((kvPairPara.WikiItemId, kvPairPara.ObjectId));
                }
            }
            var textBriefIds = briefParas.ConvertAll(x => x.textId);
            var textBriefs = (
                from ts in textSectionRepo.Existing
                where textBriefIds.Contains(ts.Id)
                select new
                {
                    ts.Id,
                    ts.ContentBrief
                }).ToList();
            var textKvPairIds = kvPairParas.ConvertAll(x => x.textId);
            var textKvPairs = (
                from ts in textSectionRepo.Existing
                where textKvPairIds.Contains(ts.Id)
                where ts.Content != null && ts.Content.Length < 500
                select new
                {
                    ts.Id,
                    ts.Content
                }).ToList();
            var resp = new WikiTopBriefOfDirResponse();
            foreach (var w in newestWs)
            {
                var briefTextId = briefParas.Find(x => x.wikiId == w.Id).textId;
                var briefText = textBriefs.Find(t => t.Id == briefTextId)?.ContentBrief;
                var kvPairTextId = kvPairParas.Find(x => x.wikiId == w.Id).textId;
                var kvPairText = textKvPairs.Find(t => t.Id == kvPairTextId)?.Content;
                if (briefText is { } || kvPairText is { })
                {
                    var pairs = ParseKvPairs(kvPairText);
                    resp.Items.Add(new()
                    {
                        Id = w.Id,
                        PathName = w.UrlPathName,
                        OwnerName = w.OwnerName,
                        Brief = briefText,
                        Time = w.LastActive.ToString("MM/dd"),
                        Title = w.Title,
                        KvPairs = pairs,
                    });
                }
            }
            return this.ApiResp(resp);
        }

        private static readonly char[] kvSep = [':', '：'];
        private static Dictionary<string, string>? ParseKvPairs(string? content)
        {
            if (content is null)
                return null;
            Dictionary<string, string> res = [];
            foreach (var line in content.Split('\n'))
            {
                var kv = line.Split(kvSep, 2);
                if (kv.Length < 2)
                    continue;
                var key = kv[0];
                var value = kv[1];
                if (!res.TryAdd(key, value))
                    res[key] = value;
            }
            return res;
        }

        public class WikiTopBriefOfDirRequest
        {
            public int DirId { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
            public int TakeBriefAt { get; set; }
            public int TakeKvPairAt { get; set; }
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
                public Dictionary<string, string>? KvPairs { get; set; }
            }
        }
    }
}
