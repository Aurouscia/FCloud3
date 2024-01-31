using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Table;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.DbContexts
{
    public class FCloudContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserToGroup> UserToGroups { get; set; }
        public DbSet<WikiItem> WikiItems { get; set; }
        public DbSet<WikiToDir> WikiToDirs { get; set; }
        public DbSet<WikiPara> WikiParas { get; set; }
        public DbSet<TextSection> TextSections { get; set; }
        public DbSet<FreeTable> FreeTables { get; set; }
        public DbSet<FileItem> FileItems { get; set; }
        public DbSet<FileDir> FileDirs { get; set; }
    }
}
