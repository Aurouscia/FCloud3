using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Services.Etc.Metadata.Abstraction;
using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.Services.Etc.Metadata
{
    public class UserMetadataService(
        UserRepo repo) 
        : MetadataServiceBase<UserMetadata, User>(repo)
    {
        protected override IQueryable<UserMetadata> GetFromDbModel(IQueryable<User> dbModels)
        {
            return dbModels.Select(x => new UserMetadata(x.Id, x.Name??"??", x.AvatarMaterialId, x.Type));
        }
        public void Create(int id, string name, UserType type)
        {
            var model = new UserMetadata(id, name, 0, type);
            base.Create(model);
        }
        public UserMetadata? GetByName(string name)
        {
            var stored = DataList.FirstOrDefault(x => x.Name == name);
            if (stored is not null)
                return stored;
            var u = GetFromDbModel(_repo.Existing.Where(x=>x.Name == name)).FirstOrDefault();
            if (u is null) return null;
            DataList.Add(u);
            return u;
        }
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
