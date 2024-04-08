using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.DbContexts
{
    public class DbTransactionService
    {
        private readonly FCloudContext _context;

        public DbTransactionService(FCloudContext context) 
        {
            _context = context;
        }

        /// <summary>
        /// 开始数据库事务
        /// </summary>
        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }
        /// <summary>
        /// 提交数据库事务
        /// </summary>
        public void CommitTransaction(IDbContextTransaction transaction)
        {
            transaction.Commit();
        }
        /// <summary>
        /// 回滚数据库事务
        /// </summary>
        public void RollbackTransaction(IDbContextTransaction transaction)
        {
            transaction.Rollback();
        }

        /// <summary>
        /// 将传入的函数作为数据库事务执行
        /// </summary>
        /// <param name="action">要执行的函数，其返回值为“是否成功”，返回false就会回滚事务</param>
        /// <returns>传入的函数的返回值</returns>
        public bool DoTransaction(Func<bool> action)
        {
            using var t = BeginTransaction();
            if (action())
            {
                CommitTransaction(t);
                return true;
            }
            RollbackTransaction(t);
            return false;
        }
    }
}
