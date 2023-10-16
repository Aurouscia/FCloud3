using FCloud3.Repos.Models.TextSec;

namespace FCloud3.App.Models.COM.TextSec
{
    public class TextSectionComModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }

        public TextSectionComModel() { }
        public TextSectionComModel(TextSection original)
        {
            Id = original.Id;
            Title = original.Title;
            Content = original.Content;
        }
    }
}
