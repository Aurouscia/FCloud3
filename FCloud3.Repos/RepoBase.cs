using FCloud3.DbContexts;
using FCloud3.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace FCloud3.Repos
{
    public abstract class RepoBase<T> where T : class,IDbModel
    {
        protected readonly FCloudContext _context;
        protected readonly ICommitingUserIdProvider _userIdProvider;

        public RepoBase(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        {
            _context = context;
            _userIdProvider = userIdProvider;
        }

        public IQueryable<T> All => _context.Set<T>();
        public IQueryable<T> Existing => _context.Set<T>().Where(x => x.Deleted == false);
        public IQueryable<T> Deleted => _context.Set<T>().Where(x => x.Deleted);
        public IQueryable<T> ExistingExceptId(int id) => Existing.Where(x => x.Id != id);
        public int ExistingCount => Existing.Count();
        public void SetUpdateTime(int id) => Existing.Where(x => x.Id == id).ExecuteUpdate(x => x.SetProperty(w => w.Updated, DateTime.Now));
        public void SetUpdateTime(List<int> ids) => Existing.Where(x=>ids.Contains(x.Id)).ExecuteUpdate(x => x.SetProperty(w => w.Updated, DateTime.Now));
        public virtual IQueryable<T> OwnedByUser(int uid = -1)
        {
            if (uid == -1)
                return Existing;
            return Existing.Where(x => x.CreatorUserId == uid);
        }

        public virtual IQueryable<T> IndexFilterOrder(IndexQuery query)
        {
            return IndexFilterOrder(Existing, query);
        }
        public virtual IQueryable<T> IndexFilterOrder(IndexQuery query, Func<string, string> keyReplace)
        {
            return IndexFilterOrder(Existing, query, keyReplace);
        }
        public virtual IQueryable<T> IndexFilterOrder(IQueryable<T> from, IndexQuery query, Func<string,string>? keyReplace = null)
        {
            var q = from;
            var parsedSearch = query.ParsedSearch();
            var orderBy = query.OrderBy;
            if (keyReplace is not null)
            {
                parsedSearch.ForEach(s => { s.Key = keyReplace(s.Key); });
                if(orderBy is not null)
                    orderBy = keyReplace(orderBy);
            }
            q = Filter(q, parsedSearch);
            q = Order(q, orderBy, query.OrderRev);
            return q;
        }
        public virtual IndexResult<T> Index(IndexQuery query)
        {
            return  IndexFilterOrder(query).TakePageAndConvertOneByOne(query,x=>x);
        }
        public virtual IQueryable<T> Filter(IQueryable<T> q, SearchDict dict)
        {
            if (dict.Count==0)
                return q;
            Type t = typeof(T);
            var props = t.GetProperties();
            Lazy<MethodInfo> stringContain = new(() =>
            {
                var contains = typeof(string).GetMethods();
                return contains.First(x =>
                {
                    if (x.Name != nameof(string.Contains) || x.ContainsGenericParameters)
                        return false;
                    var ps = x.GetParameters();
                    if (ps.Length != 1 || ps[0].ParameterType != typeof(string))
                        return false;
                    return true;
                });
            });
            foreach (var search in dict)
            {
                var p = props.FirstOrDefault(x => x.Name == search.Key);
                if (p is null) continue;

                LambdaExpression lambda;
                ParameterExpression param = Expression.Parameter(t, "x");
                MemberExpression member = Expression.Property(param, p);
                if (p.PropertyType == typeof(string))
                {
                    ConstantExpression stringValue = Expression.Constant(search.Value);

                    var contain = Expression.Call(member, stringContain.Value ,stringValue);
                    lambda = Expression.Lambda(contain, param);
                }
                else if (p.PropertyType == typeof(int))
                {
                    if (int.TryParse(search.Value, out var value))
                    {
                        ConstantExpression intValue = Expression.Constant(value);
                        lambda = Expression.Lambda(Expression.Equal(member, intValue), param);
                    }
                    else
                        continue;
                }
                else
                    continue;
                var selector = lambda as Expression<Func<T, bool>>;
                if (selector is not null)
                {
                    q = q.Where(selector);
                }
            }
            return q;
        }
        public virtual IQueryable<T> Order(IQueryable<T> q, string? orderBy, bool rev)
        {
            if (string.IsNullOrEmpty(orderBy))
                return q;
            Type t = typeof(T);
            var props = t.GetProperties();

            static IQueryable<T> sort<TBy>(IQueryable<T> q ,LambdaExpression lambda, bool rev)
            {
                var selector = lambda as Expression<Func<T, TBy>>;
                if (selector is not null)
                    q = rev ? q.OrderByDescending(selector) : q.OrderBy(selector);
                return q;
            }

            foreach(var prop in props)
            {
                Type propType = prop.PropertyType;
                if (prop.Name == orderBy)
                {
                    ParameterExpression param = Expression.Parameter(t, "x");
                    MemberExpression member = Expression.Property(param, prop);
                    LambdaExpression lambda = Expression.Lambda(member, param);
                    if (propType == typeof(string))
                        q = sort<string>(q, lambda, rev);
                    else if (propType == typeof(int))
                        q = sort<int>(q, lambda, rev);
                    else if (propType == typeof(DateTime))
                        q = sort<DateTime>(q, lambda, rev);
                }
            }
            return q;
        }

        public virtual T? GetById(int id)
        {
            return Existing.Where(x => x.Id == id).FirstOrDefault();
        }
        public virtual T GetByIdEnsure(int id)
        {
            return Existing.Where(x => x.Id == id).FirstOrDefault() ?? throw new Exception("找不到目标");
        }
        public virtual IQueryable<T> GetRangeByIds(IEnumerable<int> ids)
        {
            if (ids.Count() == 0) 
                return new List<T>().AsQueryable();
            return Existing.Where(x => ids.Contains(x.Id));
        }
        public virtual IQueryable<T> GetRangeByIds(IQueryable<int> ids)
        {
            return Existing.Where(x => ids.Contains(x.Id));
        }
        public virtual List<T2> GetRangeByIdsOrdered<T2>(IEnumerable<int> ids, Func<IQueryable<T>, Dictionary<int, T2>> converter)
        {
            var vals = converter(GetRangeByIds(ids));
            if(vals.Count == 0) 
                return [];
            var res = new List<T2>();
            foreach(var id in ids)
            {
                vals.TryGetValue(id, out T2? val);
                if(val is not null)
                {
                    res.Add(val);
                }
            }
            return res;
        }
        public virtual IQueryable<T> GetqById(int id)
        {
            return Existing.Where(x => x.Id == id);
        }

        public virtual int GetOwnerIdById(int id)
        {
            return Existing.Where(x => x.Id == id).Select(x => x.CreatorUserId).FirstOrDefault();
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
            item.CreatorUserId = _userIdProvider.Get();
            item.Created = DateTime.Now;
            item.Updated = DateTime.Now;
            _context.Add(item);
            _context.SaveChanges();
            return true;
        }
        public virtual bool TryAddRange(List<T> items, out string? errmsg)
        {
            errmsg = null;
            foreach(var item in items)
            {
                if (item is null)
                {
                    errmsg = $"试图向数据库加入空{nameof(T)}对象";
                    return false;
                }
                if (!TryAddCheck(item, out errmsg))
                    return false;
                item.Created = DateTime.Now;
                item.Updated = DateTime.Now;
                item.CreatorUserId = _userIdProvider.Get();
                _context.Add(item);
            }
            _context.SaveChanges();
            return true;
        }
        public virtual int TryAddAndGetId(T item, out string? errmsg)
        {
            _ = TryAdd(item, out errmsg);
            return item.Id;
        }
        public virtual int TryCreateDefaultAndGetId(out string? errmsg)
        {
            throw new NotImplementedException();
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
            if (!TryRemoveCheck(item, out errmsg))
                return false;
            item.Deleted = true;
            item.Updated = DateTime.Now;
            _context.Update(item);
            _context.SaveChanges();
            return true;
        }
        public virtual bool TryRemove(int id, out string? errmsg)
        {
            var deleted = Existing.Where(x => x.Id == id)
                .ExecuteUpdate(x => x
                .SetProperty(t => t.Deleted, true)
                .SetProperty(t => t.Updated, DateTime.Now));
            if (deleted > 0)
            {
                errmsg = null;
                return true;
            }
            else
            {
                errmsg = "删除失败(可能未找到指定id)";
                return false;
            }
        }
        public virtual bool TryRemoveRange(List<T> items, out string? errmsg)
        {
            errmsg = null;
            foreach (var item in items)
            {
                if (item is null)
                {
                    errmsg = $"试图向数据库删除空{nameof(T)}对象";
                    return false;
                }
                if (!TryRemoveCheck(item, out errmsg))
                    return false;
                item.Deleted = true;
                item.Updated = DateTime.Now;
                _context.Update(item);
            }
            _context.SaveChanges();
            return true;
        }

        public virtual bool TryRecover(T item, out string? errmsg)
        {
            if (item is null)
            {
                errmsg = $"试图向数据库恢复空{nameof(T)}对象";
                return false;
            }
            if (!TryAddCheck(item, out errmsg))
                return false;
            item.Deleted = false;
            item.Updated = DateTime.Now;
            _context.Update(item);
            _context.SaveChanges();
            return true;
        }
        public virtual bool TryRecover(int id, out string? errmsg)
        {
            var deleted = Existing.Where(x => x.Id == id)
                .ExecuteUpdate(x => x
                .SetProperty(t => t.Deleted, false)
                .SetProperty(t => t.Updated, DateTime.Now));
            if (deleted > 0)
            {
                errmsg = null;
                return true;
            }
            else
            {
                errmsg = "恢复失败(可能未找到指定id)";
                return false;
            }
        }
        public virtual bool TryRecoverRange(List<T> items, out string? errmsg)
        {
            errmsg = null;
            foreach (var item in items)
            {
                if (item is null)
                {
                    errmsg = $"试图向数据库恢复空{nameof(T)}对象";
                    return false;
                }
                if (!TryAddCheck(item, out errmsg))
                    return false;
                item.Deleted = false;
                item.Updated = DateTime.Now;
                _context.Update(item);
            }
            _context.SaveChanges();
            return true;
        }

        public virtual bool TryRemovePermanent(T item, out string? errmsg) 
        {
            if (item is null)
            {
                errmsg = $"试图向数据库删除空{nameof(T)}对象";
                return false;
            }
            if (!TryRemoveCheck(item, out errmsg))
                return false;
            _context.Remove(item);
            _context.SaveChanges();
            return true;
        }
        public virtual bool TryRemovePermanent(int id, out string? errmsg)
        {
            var deleted = Existing.Where(x => x.Id == id).ExecuteDelete();
            if (deleted > 0)
            {
                errmsg = null;
                return true;
            }
            else
            {
                errmsg = "删除失败(可能未找到指定id)";
                return false;
            }
        }
        public virtual bool TryRemoveRangePermanent(List<T> items, out string? errmsg)
        {
            errmsg = null;
            foreach (var item in items)
            {
                if (item is null)
                {
                    errmsg = $"试图向数据库删除空{nameof(T)}对象";
                    return false;
                }
                if (!TryRemoveCheck(item, out errmsg))
                    return false;
                _context.Remove(item);
            }
            _context.SaveChanges();
            return true;
        }
    }
}
