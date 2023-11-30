using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.DbContexts
{
    public class FCloudSqlServerContext : FCloudContext
    {
        private readonly DbContextOptions _options;
        private const string acceptDbType = "sqlserver";
        public FCloudSqlServerContext(DbContextOptions options)
        {
            if (options.Type.ToLower() != acceptDbType)
                throw new Exception($"数据库类型配置异常，应为:{acceptDbType}");
            _options = options;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_options.ConnStr);
        }
    }
}
