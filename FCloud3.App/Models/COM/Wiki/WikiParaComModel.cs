using FCloud3.Repos.Models.Wiki;

namespace FCloud3.App.Models.COM.Wiki
{
    public class WikiParaComModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public WikiParaType Type { get; set; }
    }
}
