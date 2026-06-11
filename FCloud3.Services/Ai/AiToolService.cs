using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using FCloud3.Services.WikiParsing;

namespace FCloud3.Services.Ai
{
    public class AiToolService(
        WikiParsingService wikiParsingService,
        WikiItemRepo wikiItemRepo,
        QuickSearchService quickSearchService)
    {
        /// <summary>获取指定词条的纯文本内容（用于 AI 分析）</summary>
        public string? GetWikiContent(string pathName)
        {
            var result = wikiParsingService.GetParsedWiki(pathName);
            if (result.Id == 0) return null;
            // 提取所有文本段落的纯文本
            var texts = result.Paras
                .Where(p => p.ParaType == WikiParaType.Text)
                .Select(p => $"## {p.Title}\n{p.Content}")
                .ToList();
            return string.Join("\n\n", texts);
        }

        /// <summary>根据关键词搜索词条</summary>
        public List<WikiSearchResult> SearchWiki(string keyword)
        {
            var q = wikiItemRepo.QuickSearch(keyword, false, 0);
            return q.Take(5).Select(x => new WikiSearchResult(x.Title, x.UrlPathName)).ToList();
        }
    }

    public record WikiSearchResult(string? Title, string? UrlPathName);
}
