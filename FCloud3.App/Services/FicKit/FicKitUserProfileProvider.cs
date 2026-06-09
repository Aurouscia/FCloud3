using Aurouscia.FicKit.Common.Abstractions;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Services.Identities;

namespace FCloud3.App.Services.FicKit
{
    public class FicKitUserProfileProvider(
        UserRepo userRepo,
        UserService userService,
        MaterialRepo materialRepo
        ): IUserProfileProvider
    {
        public Task<IReadOnlyList<UserProfile>> GetProfilesAsync(IReadOnlyList<int> userIds)
        {
            var users = userRepo.CachedItemsByIds([..userIds]);
            IReadOnlyList<UserProfile> result = users.ConvertAll(x => {
                var matId = x.AvatarMaterialId;
                string? matSrc = null;
                if(matId > 0)
                {
                    var matName = materialRepo.CachedItemById(matId)?.PathName;
                    matSrc = userService.AvatarFullUrl(matName);
                }
                return new UserProfile
                {
                    UserId = x.Id,
                    UserName = x.Name,
                    AvatarUrl = matSrc ?? userService.DefaultAvatar()
                };
            });
            return Task.FromResult(result);
        }
    }
}
