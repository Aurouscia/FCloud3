using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc.Metadata.Abstraction;
using FCloud3.Repos.Identities;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Etc.Metadata
{
    public class UserMetadataRepo(
        UserRepo repo,
        ILogger<MetadataRepoBase<UserMetadata, User>> logger) 
        : MetadataRepoBase<UserMetadata, User>(repo, logger)
    {
        protected override IQueryable<UserMetadata> GetFromDbModel(IQueryable<User> dbModels)
        {
            return dbModels.Select(x => new UserMetadata(x.Id, x.Name??"??", x.AvatarMaterialId, x.Type));
        }
        public void Create(int id, string name, UserType type)
        {
            var model = new UserMetadata(id, name, 0, type);
            base.Insert(model);
        }
        public UserMetadata? GetByName(string name)
        {
            var stored = DataListSearch(x => x.Name == name);
            if (stored is not null)
                return stored;
            var u = GetFromDbModel(_repo.Existing.Where(x=>x.Name == name)).FirstOrDefault();
            if (u is null)
                return null;
            base.Insert(u);
            return u;
        }

        private static readonly object LockObj = new();
        protected override object Locker => LockObj;
    }
    public class UserMetadata: MetadataBase<User>
    {
        public UserType Type { get; set; }
        public string Name { get; set; }
        public int AvatarMaterialId { get; set; }
        public UserMetadata(int id, string name, int avatarMaterialId, UserType type)
        {
            Id = id;
            Name = name;
            AvatarMaterialId = avatarMaterialId;
            Type = type;
        }
    }
}
