using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Etc.Caching.Abstraction;

namespace FCloud3.Repos
{
    public abstract class RepoBaseWithCaching<T, TCache> : RepoBase<T> 
        where T : class, IDbModel 
        where TCache : CachingModelBase<T>
    {
        protected readonly CachingBase<TCache, T> _caching;
        protected RepoBaseWithCaching(
            FCloudContext context,
            ICommitingUserIdProvider userIdProvider,
            CachingBase<TCache, T> caching) : base(context, userIdProvider)
        {
            _caching = caching;
        }

        public override bool TryAdd(T item, out string? errmsg)
        { 
            var s = base.TryAdd(item, out errmsg);
            if(s)
                _caching.Insert(item);
            return s;
        }
        public override bool TryAddRange(List<T> items, out string? errmsg)
        {
            var s = base.TryAddRange(items, out errmsg);
            if(s)
                _caching.InsertRange(items);
            return s;
        }

        public override bool TryEdit(T item, out string? errmsg, bool updateTime = true)
        {
            var s = base.TryEdit(item, out errmsg, updateTime);
            if(s)
                _caching.UpdateByDbModel(item);
            return s;
        }
        public override bool TryEditRange(List<T> items, out string? errmsg, bool updateTime = true)
        {
            var s = base.TryEditRange(items, out errmsg, updateTime);
            if(s)
                _caching.UpdateRangeByDbModel(items);
            return s;
        }
        
        public override bool TryRemove(T item, out string? errmsg)
        {
            var s = base.TryRemove(item, out errmsg);
            if(s)
                _caching.Remove(item.Id);
            return s;
        }
        public override bool TryRemoveNoCheck(int id, out string? errmsg)
        {
            var s = base.TryRemoveNoCheck(id, out errmsg);
            if(s)
                _caching.Remove(id);
            return s;
        }
        public override bool TryRemoveRange(List<T> items, out string? errmsg)
        {
            var s = base.TryRemoveRange(items, out errmsg);
            if(s)
                _caching.RemoveRange(items.ConvertAll(x=>x.Id));
            return s;
        }
        public override bool TryRemovePermanent(T item, out string? errmsg)
        {
            var s = base.TryRemovePermanent(item, out errmsg);
            if(s)
                _caching.Remove(item.Id);
            return s;
        }
        public override bool TryRemovePermanentNoCheck(int id, out string? errmsg)
        {
            var s = base.TryRemovePermanentNoCheck(id, out errmsg);
            if(s)
                _caching.Remove(id);
            return s;
        }
        public override bool TryRemoveRangePermanent(List<T> items, out string? errmsg)
        {
            var s = base.TryRemoveRangePermanent(items, out errmsg);
            if(s)
                _caching.RemoveRange(items.ConvertAll(x=>x.Id));
            return s;
        }
    }
}