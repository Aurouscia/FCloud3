using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc.Caching.Abstraction;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Etc.Caching
{
    public class AuthGrantCaching : CachingBase<AuthGrantCachingModel, AuthGrant>
    {
        public AuthGrantCaching(FCloudContext context, ILogger<CachingBase<AuthGrantCachingModel, AuthGrant>> logger) 
            : base(context, logger)
        {
        }
        public AuthGrantCaching(
            IQueryable<AuthGrant> source, 
            ILogger<CachingBase<AuthGrantCachingModel, AuthGrant>> logger) 
            : base(source, logger)
        {
        }
        protected override IQueryable<AuthGrantCachingModel> GetFromDbModel(IQueryable<AuthGrant> dbModels)
        {
            return dbModels.Select(x => new AuthGrantCachingModel()
            {
                Id = x.Id, Order = x.Order, CreatorUserId = x.CreatorUserId,
                On = x.On, OnId = x.OnId, To = x.To, ToId = x.ToId, IsReject = x.IsReject
            });
        }
        protected override AuthGrantCachingModel GetFromDbModel(AuthGrant x)
        {
            return new AuthGrantCachingModel()
            {
                Id = x.Id, Order = x.Order, CreatorUserId = x.CreatorUserId,
                On = x.On, OnId = x.OnId, To = x.To, ToId = x.ToId, IsReject = x.IsReject
            };
        }
        public AuthGrantCachingModel Convert(AuthGrant x) => GetFromDbModel(x);
        protected override void MutateByDbModel(AuthGrantCachingModel target, AuthGrant from)
        {
            target.Order = from.Order;
            target.CreatorUserId = from.CreatorUserId;
            target.On = from.On;
            target.OnId = from.OnId;
            target.To = from.To;
            target.ToId = from.ToId;
            target.IsReject = from.IsReject;
        }
    }

    public class AuthGrantCachingModel
        : CachingModelBase<AuthGrant>
    {
        public int Order { get; set; }
        public int CreatorUserId { get; set; }
        public AuthGrantOn On { get; set; }
        public int OnId { get; set; }
        public AuthGrantTo To { get; set; }
        public int ToId { get; set; }
        public bool IsReject { get; set; }
    }
}