namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class LinkOptions
    {
        public Func<LinkItem, string?, string> ConvertFn { get; set; }
        public List<LinkItem> LinkItems { get; set; }
        private readonly ParserBuilder _master;
        
        public LinkOptions(ParserBuilder master)
        {
            LinkItems = [];
            _master = master;
            ConvertFn = DefaultConvertFn;
        }
        private string DefaultConvertFn(LinkItem linkItem, string? mustUseName)
        {
            if(mustUseName is not null)
                return $"<a href=\"{linkItem.Url}\">{mustUseName}</a>";
            return $"<a href=\"{linkItem.Url}\">{linkItem.Text}</a>";
        }

        public ParserBuilder AddLinkItem(string text, string url)
        {
            LinkItems.Add(new(text, url));
            return _master;
        }

        public ParserBuilder ReplaceConvertFn(Func<LinkItem, string?, string> convertFunc)
        {
            ConvertFn = convertFunc;
            return _master;
        }
    }

    public class LinkItem(string text, string url)
    {
        public string Text { get; set; } = text;
        public string Url { get; set; } = url;
    }
}