using FCloud3.Repos.DB;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos
{
    public abstract class RepoBase<T> where T : class,IDbModel
    {
        protected readonly FCloudContext _context;
        public RepoBase(FCloudContext context)
        {
            _context = context;
        }
        public IQueryable<T> Existing { get { return _context.Set<T>().Where(x => x.Deleted == false); } }
        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }
        public void CommitTransaction()
        {
            _context.Database.CommitTransaction();
        }
        public virtual T? GetById(int id)
        {
            return Existing.Where(x => x.Id == id).FirstOrDefault();
        }

        public virtual bool TryAddCheck(T item, out string? errmsg)
        {
            errmsg = null;
            return true;
        }
        public virtual bool TryAdd(T item,out string? errmsg)
        {
            if(item is null)
            {
                errmsg = $"试图向数据库加入空{nameof(T)}对象";
                return false;
            }
            if(!TryAddCheck(item, out errmsg))
                return false;
            item.Created = DateTime.Now;
            _context.Add(item);
            _context.SaveChanges();
            return true;
        }

        public virtual bool TryEditCheck(T item, out string? errmsg)
        {
            errmsg = null;
            return true;
        }
        public virtual bool TryEdit(T item, out string? errmsg)
        {
            if (item is null)
            {
                errmsg = $"试图向数据库更新空{nameof(T)}对象";
                return false;
            }
            if (!TryEditCheck(item, out errmsg))
                return false;
            item.Updated = DateTime.Now;
            _context.Update(item);
            _context.SaveChanges();
            return true;
        }
        public virtual bool TryEditRange(List<T> items,out string? errmsg)
        {
            errmsg = null;
            foreach(var item in items)
            {
                if (item is null)
                {
                    errmsg = $"试图向数据库更新空{nameof(T)}对象";
                    return false;
                }
                if (!TryEditCheck(item, out errmsg))
                    return false;
                item.Updated = DateTime.Now;
                _context.Update(item);
            }
            _context.SaveChanges();
            return true;
        }

        public virtual bool TryRemoveCheck(T item, out string? errmsg)
        {
            errmsg = null;
            return true;
        }
        public virtual bool TryRemove(T item, out string? errmsg)
        {
            if (item is null)
            {
                errmsg = $"试图向数据库删除空{nameof(T)}对象";
                return false;
            }
            if (TryRemoveCheck(item, out errmsg))
                return false;
            item.Deleted = true;
            _context.Update(item);
            _context.SaveChanges();
            return true;
        }
    }
}
