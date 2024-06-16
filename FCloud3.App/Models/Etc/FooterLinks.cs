namespace FCloud3.App.Models.Etc
{
    public class FooterLinks
    {
        public List<FooterLink> Links { get; set; } = [];
        public void Add(string text, string? url)
        {
            Links.Add(new(text, url));
        }
        public class FooterLink(string text, string? url)
        {
            public string Text { get; set; } = text;
            public string? Url { get; set; } = url;
        }
    }
}