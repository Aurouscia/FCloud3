using FCloud3.Repos.Models.Corr;
using FCloud3.Repos.Models.Identities;
using FCloud3.Repos.Models.Wiki;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos.DB
{
    public class FCloudContext : DbContext
    {
        public FCloudContext(DbContextOptions<FCloudContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<WikiItem> WikiItems { get; set; }
        public DbSet<Corr> Corrs { get; set; }
    }
}
