using FCloud3.Entities.Identities;
using FCloud3.Repos.Identities;
using FCloud3.Services.Etc.Metadata.Abstraction;

namespace FCloud3.Services.Etc.Metadata
{
    public class UserMetadataService(UserRepo repo) : MetadataServiceBase<UserMetadata, User>(repo)
    {
        protected override IQueryable<UserMetadata> GetFromDbModel(IQueryable<User> dbModels)
        {
            return dbModels.Select(x => new UserMetadata(x.Id, x.Type));
        }
        public void Create(int id, UserType type)
        {
            var model = new UserMetadata(id, type);
            base.Create(model);
        }
    }
    public class UserMetadata: MetadataBase<User>
    {
        public UserType Type { get; set; }
        public UserMetadata(int id, UserType type)
        {
            Id = id;
            Type = type;
        }
    }
}
