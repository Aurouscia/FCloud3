using FCloud3.Entities.DbModels.Corr;
using FCloud3.Entities.DbModels.Identities;
using FCloud3.Entities.DbModels.TextSec;
using FCloud3.Entities.DbModels.Wiki;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.DbContexts
{
    public class FCloudContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<WikiItem> WikiItems { get; set; }
        public DbSet<TextSection> TextSections { get; set; }
        public DbSet<Corr> Corrs { get; set; }
    }
}
