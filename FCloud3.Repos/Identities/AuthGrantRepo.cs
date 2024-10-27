using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Identities
{
    public class AuthGrantRepo : RepoBaseCache<AuthGrant, AuthGrantCacheModel>
    {
        public AuthGrantRepo(
            FCloudContext context, 
            ICommitingUserIdProvider userIdProvider) 
            : base(context, userIdProvider)
        {
        }
        /// <summary>
        /// 获取某对象的所有授权，本地和全局的/仅全局的，但不包括内置的
        /// </summary>
        /// <param name="on">对象类型</param>
        /// <param name="onId">对象id</param>
        /// <param name="owner">对象拥有者</param>
        /// <returns></returns>
        public List<AuthGrantCacheModel> GetByOn(AuthGrantOn on, int onId, int owner)
        {
            var items = AllCachedItems();
            //要么直接在对象上，要么被所有者定义为“所有我的”的
            items = items.Where(x =>
                x.On == on && (x.OnId == onId 
                || (x.OnId == AuthGrant.onIdForAll && x.CreatorUserId == owner)));

            //如果要的就是“所有我的”，那么只返回当前登录用户的，无视owner参数
            if (onId == AuthGrant.onIdForAll)
                items = items.Where(x => x.CreatorUserId == _userIdProvider.Get());
            var res = items.ToList();
            res.Sort((x, y) =>
            {
                var xIsAll = x.OnId == AuthGrant.onIdForAll ? 1 : 0;
                var yIsAll = y.OnId == AuthGrant.onIdForAll ? 1 : 0;
                if (xIsAll != yIsAll)
                    return yIsAll - xIsAll;
                return x.Order - y.Order;
            });
            return res;
        }
        
        /// <summary>
        /// 获取某对象/某用户的所有授权，仅本地的/仅全局的
        /// </summary>
        /// <param name="on"></param>
        /// <param name="onId"></param>
        /// <returns></returns>
        private List<AuthGrant> GetByOnLocal(AuthGrantOn on, int onId)
        {
            var q = Existing
                .Where(x => x.On == on)
                .Where(x => x.OnId == onId);
            if (onId == AuthGrant.onIdForAll)
                q = q.Where(x=>x.CreatorUserId == _userIdProvider.Get());
            return q
                .OrderBy(x => x.Order)
                .ToList();
        }
        public bool TryAdd(AuthGrant item, out string? errmsg)
        {
            var sameOn = GetByOnLocal(item.On, item.OnId);
            if (sameOn.Count >= AuthGrant.maxCountOnSameOn)
                errmsg = "不允许设置数量过多";
            if (sameOn.Count != 0)
            {
                sameOn.EnsureOrderDense();
                UpdateRange(sameOn);
                var newOrder = sameOn.Select(x => x.Order).Max() + 1;
                item.Order = newOrder;
            }
            else
            {
                item.Order = 0;
            }
            base.Add(item);
            errmsg = null;
            return true;
        }
        public bool TryRemove(AuthGrant item, out string? errmsg)
        {
            var sameOn = GetByOnLocal(item.On, item.OnId);
            if (!sameOn.Contains(item))
            {
                errmsg = "找不到要删除的对象";
                return false;
            }
            sameOn.RemoveAll(x=>x.Id == item.Id);
            sameOn.EnsureOrderDense();
            UpdateRange(sameOn);
            base.Remove(item);
            errmsg = null; 
            return true;
        }

        protected override IQueryable<AuthGrantCacheModel> ConvertToCacheModel(IQueryable<AuthGrant> q)
        {
            return q.Select(x => new AuthGrantCacheModel(
                x.Id, x.Updated, x.Order, x.CreatorUserId,
                x.OnId, x.On, x.ToId, x.To, x.IsReject));
        }
    }

    public class AuthGrantCacheModel(
        int id, DateTime updated,
        int order, int creatorUserId,
        int onId, AuthGrantOn on,
        int toId, AuthGrantTo to,
        bool isReject) 
    : CacheModelBase<AuthGrant>(id, updated)
    {
        public int Order { get; } = order;
        public int CreatorUserId { get; } = creatorUserId;
        public AuthGrantOn On { get; set; } = on;
        public int OnId { get; set; } = onId;
        public AuthGrantTo To { get; set; } = to;
        public int ToId { get; set; } = toId;
        public bool IsReject { get; set; } = isReject;
    }
}
