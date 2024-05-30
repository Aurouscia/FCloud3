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
        /// 获取某对象的所有授权，包括本地和全局的，但不包括内置的
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
            
            //如果要的就是“所有我的”，那么只返回当前登录用户的
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
        public List<AuthGrant> GetByOnLocal(AuthGrantOn on, int onId)
        {
            return Existing
                .Where(x => x.On == on)
                .Where(x => x.OnId == onId)
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
            sameOn.Remove(item);
            sameOn.EnsureOrderDense();
            return base.TryRemovePermanent(item, out errmsg);
        }
    }
}
