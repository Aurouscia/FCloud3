namespace FCloud3.Repos.Etc.Index
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
            return SearchPair.ParseRange(Search);
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
            if (query is not null)
            {
                query.SelfCheck();
                pageIdx = query.Page;
                pageSize = query.PageSize;
            }

            pageCount = qCount / pageSize;
            if (qCount % pageSize != 0 || pageCount == 0)
                pageCount += 1;
            if (pageIdx > pageCount)
            {
                if (returnEmptyIfExceeded)
                    return new List<TModel>().AsQueryable();
                pageIdx = pageCount;
            }

            int skip = (pageIdx - 1) * pageSize;
            int take = pageSize;
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
            if (string.IsNullOrWhiteSpace(searchStr))
                return null;
            string[] splited = searchStr.Split('=');
            if (splited.Length != 2 || splited.Any(string.IsNullOrEmpty))
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
            foreach (var s in search)
            {
                var sp = Parse(s);
                if (sp is not null)
                    res.Add(sp);
            }
            return res;
        }
    }
    public class SearchDict : List<SearchPair>
    {
        public bool TryGetValue(string key, out string value)
        {
            var pair = Find(x => x.Key == key);
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
