using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Identities
{
    public class AuthGrantRepo : RepoBase<AuthGrant>
    {
        public AuthGrantRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }
        /// <summary>
        /// 获取某对象的所有授权，本地和全局的/仅全局的，但不包括内置的
        /// </summary>
        /// <param name="on">对象类型</param>
        /// <param name="onId">对象id</param>
        /// <param name="owner">对象拥有者</param>
        /// <returns></returns>
        public List<AuthGrant> GetByOn(AuthGrantOn on, int onId, int owner)
        {
            //要么直接在对象上，要么被所有者定义为“所有我的”的
            var q = Existing
                .Where(x => x.On == on)
                .Where(x => x.OnId == onId || (x.OnId == AuthGrant.onIdForAll && x.CreatorUserId == owner));
            
            //如果要的就是“所有我的”，那么只返回当前登录用户的，无视owner参数
            if (onId == AuthGrant.onIdForAll)
                q = q.Where(x => x.CreatorUserId == _userIdProvider.Get());
            var res = q.ToList();
            res.Sort((x, y) =>
            {
                var xIsAll = x.OnId == AuthGrant.onIdForAll ? 1 : 0;
                var yIsAll = y.OnId == AuthGrant.onIdForAll ? 1 : 0;
                if(xIsAll != yIsAll)
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
        public List<AuthGrant> GetByOnLocal(AuthGrantOn on, int onId)
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
        public override bool TryAdd(AuthGrant item, out string? errmsg)
        {
            var sameOn = GetByOnLocal(item.On, item.OnId);
            if (sameOn.Count >= AuthGrant.maxCountOnSameOn)
                errmsg = "不允许设置数量过多";
            if (sameOn.Count != 0)
            {
                sameOn.EnsureOrderDense();
                if (!TryEditRange(sameOn, out errmsg))
                    return false;
                var newOrder = sameOn.Select(x => x.Order).Max() + 1;
                item.Order = newOrder;
            }
            else
            {
                item.Order = 0;
            }
            return base.TryAdd(item, out errmsg);
        }
        public override bool TryRemove(AuthGrant item, out string? errmsg)
        {
            var sameOn = GetByOnLocal(item.On, item.OnId);
            if (!sameOn.Contains(item))
            {
                errmsg = "找不到要删除的对象";
                return false;
            }
            sameOn.RemoveAll(x=>x.Id == item.Id);
            sameOn.EnsureOrderDense();
            if (!TryEditRange(sameOn, out errmsg))
                return false;
            return base.TryRemovePermanent(item, out errmsg);
        }
        public override bool TryRemoveNoCheck(int id, out string? errmsg)
            => TryRemove(GetById(id), out errmsg);
        public override bool TryRemovePermanent(AuthGrant item, out string? errmsg)
            => TryRemove(item, out errmsg);
        public override bool TryRemovePermanentNoCheck(int id, out string? errmsg)
            => TryRemoveNoCheck(id, out errmsg);
        public override bool TryRemoveRange(List<AuthGrant> items, out string? errmsg)
            => throw new InvalidOperationException();
        public override bool TryRemoveRangePermanent(List<AuthGrant> items, out string? errmsg)
            => throw new InvalidOperationException();
    }
}
