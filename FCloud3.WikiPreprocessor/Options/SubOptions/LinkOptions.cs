using FCloud3.WikiPreprocessor.DataSource.Models;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class LinkOptions
    {
        public LinkConvertFn ConvertFn { get; private set; }
        private readonly ParserBuilder _master;
        public LinkOptions(ParserBuilder master)
        {
            _master = master;
            ConvertFn = DefaultConvertFn;
        }

        private string DefaultConvertFn(LinkItem linkItem, string? mustUseName)
        {
            if(mustUseName is not null)
                return $"<a href=\"{linkItem.Url}\">{mustUseName}</a>";
            return $"<a href=\"{linkItem.Url}\">{linkItem.Text}</a>";
        }

        public ParserBuilder ReplaceConvertFn(LinkConvertFn convertFn)
        {
            ConvertFn = convertFn;
            return _master;
        }
    }

    public delegate string LinkConvertFn(LinkItem matched, string? nameOverride);
}