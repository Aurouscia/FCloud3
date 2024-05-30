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
        public List<AuthGrant> GetByOn(AuthGrantOn on, int onId)
        {
            var res = Existing
                .Where(x => x.On == on)
                .Where(x => x.OnId == onId || x.OnId == AuthGrant.onIdForAll)
                .ToList();
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
        public List<AuthGrant> GetByOnGlobal(AuthGrantOn on)
        {
            return Existing
                .Where(x => x.On == on)
                .Where(x => x.OnId == AuthGrant.onIdForAll)
                .OrderBy(x => x.Order)
                .ToList();
        }
        public override bool TryAdd(AuthGrant item, out string? errmsg)
        {
            var sameOn = GetByOn(item.On, item.OnId);
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
            var sameOn = GetByOn(item.On, item.OnId);
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
