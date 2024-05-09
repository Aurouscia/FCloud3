namespace FCloud3.Repos
{
    public class IndexQuery
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? OrderBy { get; set; }
        public bool OrderRev { get; set; }
        public List<string>? Search { get; set; }
        public SearchDict ParsedSearch()
        {
            return SearchPair.ParseRange(this.Search);
        }
        public void SelfCheck()
        {
            if (Page < 1)
                Page = 1;
            if (PageSize < 5)
                PageSize = 5;
            if (PageSize > 30)
                PageSize = 30;
        }

        public IndexQuery() { }
        public IndexQuery(bool disabled) { Disabled = disabled; }
        public bool Disabled { get; private set; } = false;
        public IndexQuery(int skipLess, int takeLess) {  SkipLess = skipLess; TakeLess = takeLess; }
        public int SkipLess { get; private set; } = 0;
        public int TakeLess { get; private set; } = 0;

        /// <summary>
        /// 当几种东西共用一个Index时，查询完某种东西后，从原始查询对象取得用于查询下一个东西的对象
        /// </summary>
        /// <param name="resBefore">之前查询的结果</param>
        /// <returns>用于下一种东西的新的查询对象</returns>
        public IndexQuery AdvanceWith(List<IndexResult?> resBefore)
        {
            if(resBefore is null || resBefore.Count == 0)
                return this;
            var totalCountBefore = resBefore.Select(x => x?.TotalCount ?? 0).Sum();
            if(totalCountBefore == 0)
                return this;

            //如果之前几次查询的东西的总量超出了前面所有页+我这页的范围，那就说明还没轮到本东西，返回一个disabled的对象，后面都不用做了
            if (totalCountBefore > Page * PageSize)
                return new(true);

            //之前查询东西形成的“完整页”数量
            var totalCompletePagesBefore = totalCountBefore / PageSize;
            //之前查询东西超出“完整页”的部分
            var exceededCompletedPages = totalCountBefore % PageSize;
            //之前查询东西的结果，将会返回的数据总量
            var displayingCount = resBefore.Select(x => x?.Data.Length ?? 0).Sum();
            if (displayingCount > 0)
            {
                //本页就在之前查询结果的末尾，后面剩下的部分可以放本东西的开头(newPage=1)，少Take一些避免超出页尺寸
                var newPage = 1;
                var takeLess = displayingCount;
                return GetCloned(newPage, 0, takeLess);
            }
            else
            {
                //本页已经超过之前查询结果的末尾，本东西的查询需要照顾到之前查询结果超出“完整页”部分，少Skip一些
                var newPage = Page - totalCompletePagesBefore;
                var skipLess = exceededCompletedPages;
                return GetCloned(newPage, skipLess, 0);
            }
        }

        private IndexQuery GetCloned(int newPage, int skipLess = 0, int takeLess = 0)
        {
            return new IndexQuery(skipLess, takeLess)
            {
                Page = newPage,
                PageSize = PageSize,
                OrderBy = OrderBy,
                OrderRev = OrderRev,
                Search = Search
            };
        }
    }

    public static class QueryablePagingExtension
    {
        public static IQueryable<TModel> TakePage<TModel>(
            this IQueryable<TModel> q, IndexQuery? query,
            out int totalCount, out int pageIdx, out int pageCount, bool returnEmptyIfExceeded = false)
        {
            int qCount = q.Count();
            totalCount = qCount;

            pageIdx = 1;
            int pageSize = 30;
            int skipLess = 0;
            int takeLess = 0;
            if (query is not null)
            {
                query.SelfCheck();
                pageIdx = query.Page;
                skipLess = query.SkipLess;
                takeLess = query.TakeLess;
                pageSize = query.PageSize;
            }

            pageCount = qCount / pageSize;
            if (qCount % pageSize != 0 || pageCount == 0)
                pageCount += 1;
            if (pageIdx > pageCount)
            {
                if(returnEmptyIfExceeded)
                    return new List<TModel>().AsQueryable();
                pageIdx = pageCount;
            }

            int skip = (pageIdx - 1) * pageSize - skipLess;
            int take = pageSize - takeLess;
            q = q.Skip(skip).Take(take);
            return q;
        }
        public static IndexResult<TDisplay> TakePageAndConvertOneByOne<TModel, TDisplay>(
            this IQueryable<TModel> q, IndexQuery? query, Func<TModel, TDisplay> map, bool returnEmptyIfExceeded = false)
        {
            q = q.TakePage(query, out int totalCount, out int pageIdx, out int pageCount, returnEmptyIfExceeded);
            var list = q.ToList();

            var mapped = list.ConvertAll(x => map(x));

            return new(mapped, pageIdx, pageCount, totalCount);
        }
        public static IndexResult<TDisplay> TakePageAndConvertAll<TModel, TDisplay>(
            this IQueryable<TModel> q, IndexQuery? query, Func<List<TModel>, List<TDisplay>> mapRange, bool returnEmptyIfExceeded = false)
        {
            q = q.TakePage(query, out int totalCount, out int pageIdx, out int pageCount, returnEmptyIfExceeded);
            var list = q.ToList();

            var mapped = mapRange(list);

            return new(mapped, pageIdx, pageCount, totalCount);
        }
    }
    public class SearchPair
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public SearchPair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public static SearchPair? Parse(string searchStr)
        {
            if(string.IsNullOrWhiteSpace(searchStr))
                return null;
            string[] splited = searchStr.Split('=');
            if(splited.Length!=2 || splited.Any(string.IsNullOrEmpty))
            {
                return null;
            }
            return new SearchPair(splited[0], splited[1]);
        }
        public static SearchDict ParseRange(IEnumerable<string>? search)
        {
            SearchDict res = new();
            if (search is null)
                return res;
            foreach(var s in search)
            {
                var sp = SearchPair.Parse(s);
                if (sp is not null)
                    res.Add(sp);
            }
            return res;
        }
    }
    public class SearchDict:List<SearchPair>
    {
        public bool TryGetValue(string key,out string value)
        {
            var pair = this.Find(x => x.Key == key);
            if (pair is null)
            {
                value = string.Empty;
                return false;
            }
            else
            {
                value = pair.Value;
                if (string.IsNullOrEmpty(value))
                    return false;
                return true;
            }
        }
    }
}
