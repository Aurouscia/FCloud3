using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.DbContexts.DbSpecific
{
    /// <summary>
    /// 内存内数据库，每次进程终止都会造成数据清空
    /// </summary>
    public class FCloudMemoryContext(DbContextOptions options) : FCloudContext(options)
    {
        public static FCloudMemoryContext Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var contextOptions = new DbContextOptionsBuilder<FCloudContext>()
                .UseSqlite(connection)
                .Options;
            var ctx = new FCloudMemoryContext(contextOptions);
            ctx.Database.EnsureCreated();
            return ctx;
        }
    }
}
