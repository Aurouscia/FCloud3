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
        public void Create(int id, string name, UserType type)
        {
            var model = new UserCachingModel(id, name, 0, type);
            Insert(model);
        }
        public UserCachingModel? GetByName(string name)
        {
            var stored = DataListSearch(x => x.Name == name);
            if (stored is not null)
                return stored;
            var u = GetFromDbModel(_dbExistingQ.Where(x => x.Name == name)).FirstOrDefault();
            if (u is null)
                return null;
            Insert(u);
            return u;
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
