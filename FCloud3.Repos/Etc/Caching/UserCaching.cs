using FCloud3.DbContexts;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc.Caching.Abstraction;
using FCloud3.Repos.Identities;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Etc.Caching
{
    public class UserCaching(
        FCloudContext ctx,
        ILogger<CachingBase<UserCachingModel, User>> logger)
        : CachingBase<UserCachingModel, User>(ctx, logger)
    {
        protected override IQueryable<UserCachingModel> GetFromDbModel(IQueryable<User> dbModels)
        {
            return dbModels.Select(x => new UserCachingModel(x.Id, x.Name ?? "??", x.AvatarMaterialId, x.Type));
        }
        protected override UserCachingModel GetFromDbModel(User model)
        {
            return new(model.Id, model.Name??"??", model.AvatarMaterialId, model.Type);
        }
        protected override void MutateByDbModel(UserCachingModel target, User from)
        {
            target.Name = from.Name??"??";
            target.AvatarMaterialId = from.AvatarMaterialId;
            target.Type = from.Type;
        }
        public UserCachingModel? GetByName(string name)
        {
            return GetCustom(
                x => x.Name == name, 
                x=>x.Where(u=>u.Name==name));
        }
    }
    public class UserCachingModel : CachingModelBase<User>
    {
        public UserType Type { get; set; }
        public string Name { get; set; }
        public int AvatarMaterialId { get; set; }
        public UserCachingModel(int id, string name, int avatarMaterialId, UserType type)
        {
            Id = id;
            Name = name;
            AvatarMaterialId = avatarMaterialId;
            Type = type;
        }
    }
}
