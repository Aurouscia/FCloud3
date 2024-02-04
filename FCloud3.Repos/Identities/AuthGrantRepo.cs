using FCloud3.DbContexts;
using FCloud3.Entities;
using FCloud3.Entities.Identities;

namespace FCloud3.Repos.Identities
{
    public class AuthGrantRepo : RepoBase<AuthGrant>
    {
        public AuthGrantRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }
        public List<AuthGrant> GetByOn(AuthGrantOn on, int onId)
        {
            return Existing.OrderByDescending(x => x.Order).Where(x => x.On == on && x.OnId == onId).ToList();
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
    }
}
