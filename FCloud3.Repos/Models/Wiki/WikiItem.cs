using FCloud3.Repos.DB;
using FCloud3.Repos.Models.Cor;
using FCloud3.Repos.Models.TextSec;

namespace FCloud3.Repos.Models.Wiki
{
    public class WikiItem : IDbModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int OwnerUserId { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public class WikiItemRepo : RepoBase<WikiItem>
    {
        public WikiItemRepo(FCloudContext context) : base(context)
        {
        }
    }
}
