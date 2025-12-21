namespace FCloud3.WikiPreprocessor.ConvertingProvider.Models
{
    public class LinkItem(string text, string url)
    {
        public string Text { get; } = text;
        public string Url { get; } = url;
    }
}
