﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.DbContexts
{
    public class FCloudSqliteDevContext : FCloudContext
    {
        private readonly DbContextOptions _options;
        private const string acceptDbType = "sqlitedev";
        public FCloudSqliteDevContext(DbContextOptions options)
        {
            if (options.Type?.ToLower() != acceptDbType)
                throw new Exception($"数据库类型配置异常，应为:{acceptDbType}");
            _options = options;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_options.ConnStr);
        }
    }
}
