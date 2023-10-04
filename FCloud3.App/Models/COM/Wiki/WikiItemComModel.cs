namespace FCloud3.App.Models.COM.Wiki
{
    public class WikiItemComModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }

    public class WikiItemParaOrdersComModel
    {
        public int Id { get; set; }
        public List<int>? OrderedParaIds { get; set; }
    }
}
