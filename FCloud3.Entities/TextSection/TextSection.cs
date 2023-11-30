namespace FCloud3.Entities.TextSection
{
    public class TextSection : IDbModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? ContentBrief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
