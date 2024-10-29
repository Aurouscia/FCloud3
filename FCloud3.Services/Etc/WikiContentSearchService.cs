using FCloud3.Diff.Display;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Etc
{
    public class WikiContentSearchService(
        TextSectionRepo textSectionRepo,
        FreeTableRepo freeTableRepo,
        WikiParaRepo wikiParaRepo,
        WikiItemRepo wikiItemRepo)
    {
        private const int maxMatchesInOnePara = 10;
        public WikiContentSearchResult Search(string str, bool includingSealed)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new();
            var texts = textSectionRepo.Search(str);
            var freeTables = freeTableRepo.Search(str);
            List<(WikiParaType type, int objId)> related = [];
            List<(WikiParaType type, int objId, string? title, string? content, List<int[]> matches)> extracted = [];
            texts.ForEach(t =>
            {
                var founds = Extract(t.Content, str);
                extracted.Add((WikiParaType.Text, t.Id, t.Title, t.Content, founds));
                related.Add((WikiParaType.Text, t.Id));
            });
            freeTables.ForEach(t =>
            {
                var founds = Extract(t.Data, str);
                extracted.Add((WikiParaType.Table, t.Id, t.Name, t.Data, founds));
                related.Add((WikiParaType.Table, t.Id));
            });
            var paras = wikiParaRepo.ParaContainingThem(related);
            var wikiIds = paras.Select(x => x.WikiItemId).Distinct();
            
            var res = new WikiContentSearchResult();
            foreach (var wId in wikiIds)
            {
                var w = wikiItemRepo.CachedItemById(wId);
                if (w is null || (!includingSealed && w.Sealed))
                    continue;

                var item = new WikiContentSearchResult.WikiContentSearchResultWikiItem(w.UrlPathName, w.Title);
                var itsParas = paras.FindAll(p => p.WikiItemId == w.Id);
                foreach (var p in itsParas)
                {
                    var extracts = 
                        extracted.FindAll(x => x.type == p.Type && x.objId == p.ObjectId);
                    if(extracts.Count == 0)
                        continue;
                    var name = extracts.First().title;
                    var paraTitle = p.NameOverride ?? name ?? "未命名段落";
                    var displays = new List<DiffDisplayFrag>();
                    foreach (var e in extracts)
                    {
                        if(string.IsNullOrWhiteSpace(e.content))
                            continue;
                        var madeSpans = CutSpan.Make(e.matches, e.content.Length, 10);
                        madeSpans.ForEach(m =>
                        {
                            displays.Add(new(m, e.content.ToList()));
                        });
                    }
                    var paraRes = 
                        new WikiContentSearchResult.WikiContentSearchResultWikiPara(paraTitle, displays);
                    item.Paras.Add(paraRes);
                }
                res.WikiItems.Add(item);
            }
            res.WikiItems.Sort((x, y) =>
            {
                var xCount = x.Paras.Select(p => p.Took.Count).Sum();
                var yCount = y.Paras.Select(p => p.Took.Count).Sum();
                return yCount - xCount;
            });
            return res;
        }

        private List<int[]> Extract(string? content, string str)
        {
            if (string.IsNullOrWhiteSpace(content))
                return [];
            int pointer = 0;
            List<int[]> founds = [];
            for (int i = 0; i < maxMatchesInOnePara; i++)
            {
                int matchedIdx = content.IndexOf(str, pointer, StringComparison.OrdinalIgnoreCase);
                if(matchedIdx == -1)
                    break;
                founds.Add([matchedIdx, matchedIdx + str.Length]);
                pointer = matchedIdx + str.Length;
            }
            return founds;
        }

        public class WikiContentSearchResult
        {
            public List<WikiContentSearchResultWikiItem> WikiItems { get; set; } = [];
            public class WikiContentSearchResultWikiItem
                (string? wikiUrlPathName, string? wikiTitle)
            {
                public string WikiUrlPathName { get; set; } = wikiUrlPathName ?? "??";
                public string WikiTitle { get; set; } = wikiTitle ?? "??";
                public List<WikiContentSearchResultWikiPara> Paras { get; set; } = [];
            }
            public class WikiContentSearchResultWikiPara
                (string paraTitle, List<DiffDisplayFrag> took)
            {
                public string ParaTitle { get; set; } = paraTitle;
                public List<DiffDisplayFrag> Took { get; set; } = took;
            }
        }
    }
}