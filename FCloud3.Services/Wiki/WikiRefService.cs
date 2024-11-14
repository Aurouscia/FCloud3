using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Wiki
{   
    /// <summary>
    /// 词条的标题/路径名更新，造成引用它的词条(显式或隐式)必须重新解析(设置Updated字段为现在)
    /// 本类用于在前者更新后，更新后者
    /// </summary>
    public class WikiRefService(
        WikiItemRepo wikiItemRepo,
        WikiRefRepo wikiRefRepo,
        WikiParaRepo wikiParaRepo,
        WikiTitleContainRepo titleContainRepo)
    {

        public void ReferencedWikiPropChangeHandle(
            int wikiId, string? originalTitle, string? originalPathName,
            string? newTitle, string? newPathName)
        {
            var refingWikiIds = wikiRefRepo
                .GetRefingWikiIds(originalTitle, originalPathName, newTitle, newPathName)
                .ToList();
            ReferencedWikiPropChangeHandle(wikiId, refingWikiIds);
        }

        public void ReferencedWikiPropChangeHandle(
            int wikiId, string? title, string? pathName)
        {
            var refingWikiIds = wikiRefRepo
                .GetRefingWikiIds(title, pathName)
                .ToList();
            ReferencedWikiPropChangeHandle(wikiId, refingWikiIds);
        }

        private void ReferencedWikiPropChangeHandle(int wikiId, List<int> refingWikiIds)
        {
            var containingParas = titleContainRepo.GetWikiParasByContaining(wikiId).ToList();
            var containingWikis = wikiParaRepo.WikisContainingThem(containingParas);
            var needUpdateTimeWikiIds = containingWikis.Union(refingWikiIds).ToList();
            wikiItemRepo.UpdateTimeAndLu(needUpdateTimeWikiIds);
        }

        public void ReferencedMaterialPropChangeHandle(string? originalName, string? newName = null)
        {
            var refingWikiIds = wikiRefRepo
                .GetRefingWikiIds(originalName, newName)
                .ToList();
            wikiItemRepo.UpdateTimeAndLu(refingWikiIds);
        }
    }
}
