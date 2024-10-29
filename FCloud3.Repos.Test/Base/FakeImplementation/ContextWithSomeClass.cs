using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Test.Base.FakeImplementation
{
    internal class FCloudContextWithSomeClass : FCloudMemoryContext
    {
        public FCloudContextWithSomeClass(DbContextOptions options) : base(options)
        {
        }

        public DbSet<SomeClass> SomeClassSet { get; set; }

        public new static FCloudContextWithSomeClass Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var contextOptions = new DbContextOptionsBuilder<FCloudContextWithSomeClass>()
                .UseSqlite(connection)
                .Options;
            var ctx = new FCloudContextWithSomeClass(contextOptions);
            ctx.Database.EnsureCreated();
            return ctx;
        }
    }
}
