using FCloud3.Entities;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Identities
{
    public class AuthGrantService
    {
        private readonly AuthGrantRepo _authGrantRepo;
        private readonly UserToGroupRepo _userToGroupRepo;
        private readonly UserGroupRepo _userGroupRepo;
        private readonly UserRepo _userRepo;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly FileDirRepo _fileDirRepo;
        private readonly IOperatingUserIdProvider _userIdProvider;

        public AuthGrantService(
            AuthGrantRepo authGrantRepo,
            UserToGroupRepo userToGroupRepo,
            UserGroupRepo userGroupRepo,
            UserRepo userRepo,
            WikiItemRepo wikiItemRepo,
            FileDirRepo fileDirRepo,
            IOperatingUserIdProvider userIdProvider)
        {
            _authGrantRepo = authGrantRepo;
            _userToGroupRepo = userToGroupRepo;
            _userGroupRepo = userGroupRepo;
            _userRepo = userRepo;
            _wikiItemRepo = wikiItemRepo;
            _fileDirRepo = fileDirRepo;
            _userIdProvider = userIdProvider;
        }
        public bool Test(AuthGrantOn on, int onId)
        {
            int userId = _userIdProvider.Get();
            if (userId == 0)
                return false;

            var gs = _authGrantRepo.GetByOn(on, onId);//按order从下到上的顺序，下面覆盖上面，所以先检验
            var ownerId = GetOwnerId(on, onId);
            if (userId == ownerId)
                return true;

            var groupIds = gs.Where(x => x.To == AuthGrantTo.UserGroup).Select(x => x.ToId).ToList();
            var groupDict = _userToGroupRepo.GetUserIdDicByGroupIds(groupIds);

            foreach (var g in gs)
            {
                if (g.To == AuthGrantTo.EveryOne)
                {
                    return !g.IsReject;
                }
                if (g.To == AuthGrantTo.User)
                {
                    if (g.ToId == userId)
                        return !g.IsReject;
                }
                if (g.To == AuthGrantTo.UserGroup)
                {
                    groupDict.TryGetValue(g.ToId, out var uids);
                    if (uids is not null && uids.Contains(userId))
                    {
                        return !g.IsReject;
                    }
                }
            }
            return _userToGroupRepo.IsInSameGroup(ownerId, userId);
        }

        public List<AuthGrantViewModel> GetList(AuthGrantOn on, int onId)
        {
            var list = _authGrantRepo.GetByOn(on, onId);
            var groupIds = list.Where(x => x.To == AuthGrantTo.UserGroup).Select(x=>x.ToId).ToList();
            var userIds = list.Where(x => x.To == AuthGrantTo.User).Select(x => x.ToId).ToList();
            var creatorIds = list.Select(x => x.CreatorUserId).ToList();
            userIds = userIds.Union(creatorIds).ToList();

            var groupNames = _userGroupRepo.GetRangeByIds(groupIds).Select(x => new { x.Id,x.Name}).ToList();
            var userNames = _userRepo.GetRangeByIds(userIds).Select(x => new { x.Id, x.Name }).ToList();
            return list.ConvertAll(x =>
            {
                string? toName = null;
                if (x.To == AuthGrantTo.UserGroup)
                    toName = groupNames.FirstOrDefault(g => g.Id == x.ToId)?.Name;
                else if (x.To == AuthGrantTo.User)
                    toName = userNames.FirstOrDefault(u => u.Id == x.ToId)?.Name;
                else if (x.To == AuthGrantTo.EveryOne)
                    toName = "所有人";
                toName ??= "N/A";
                string creatorName = userNames.FirstOrDefault(u=>u.Id==x.CreatorUserId)?.Name ?? "N/A";
                return new AuthGrantViewModel(x, toName, creatorName);
            });
        }
        public bool Add(AuthGrant newGrant, out string? errmsg)
        {
            int userId = _userIdProvider.Get();
            int owner = GetOwnerId(newGrant.On, newGrant.OnId);
            if (userId != owner)
            {
                errmsg = "只有所有者能设置权限";
                return false;
            }
            _authGrantRepo.TryAdd(newGrant, out errmsg);
            return true;
        }
        public bool Remove(int id, out string? errmsg)
        {
            int userId = _userIdProvider.Get();
            AuthGrant? target = _authGrantRepo.GetById(id);
            if (target is null)
            {
                errmsg = "找不到指定目标，请刷新后重试";
                return false;
            }
            int owner = GetOwnerId(target.On, target.OnId);
            if (userId != owner)
            {
                errmsg = "只有所有者能设置权限";
                return false;
            }
            return _authGrantRepo.TryRemove(target, out errmsg);
        }
        public bool SetOrder(AuthGrantOn on, int onId, List<int> ids, out string? errmsg)
        {
            if (ids.Count == 0) {
                errmsg = null;
                return true;
            }
            int userId = _userIdProvider.Get();
            int owner = GetOwnerId(on, onId);
            if (userId != owner)
            {
                errmsg = "只有所有者能设置权限";
                return false;
            }
            if (ids.Count > AuthGrant.maxCountOnSameOn)
            {
                errmsg = "数量超出上限";
                return false;
            }
            var gs = _authGrantRepo.GetRangeByIds(ids).ToList();
            if (gs.Count != ids.Count)
            {
                errmsg = "数据异常，请刷新后重试";
                return false;
            }
            if (!gs.All(x => x.On == on))
            {
                errmsg = "数据异常，请刷新后重试";
                return false;
            }
            if (!gs.All(x => x.OnId == onId))
            {
                errmsg = "数据异常，请刷新后重试";
                return false;
            }
            gs.ResetOrder(ids);
            return _authGrantRepo.TryEditRange(gs, out errmsg);
        }

        private int GetOwnerId(AuthGrantOn on, int onId)
        {
            if (on == AuthGrantOn.WikiItem)
            {
                return _wikiItemRepo.GetOwnerIdById(onId);
            }
            else if (on == AuthGrantOn.Dir)
            {
                return _fileDirRepo.GetOwnerIdById(onId);
            }
            return 0;
        }

        public class AuthGrantViewModel:AuthGrant
        {
            public string ToName { get; }
            public string CreatorName { get; }
            public AuthGrantViewModel(AuthGrant authGrant, string toName, string creatorName)
            {
                this.Id = authGrant.Id;
                this.ToId = authGrant.ToId;
                this.To = authGrant.To;
                this.OnId = authGrant.OnId;
                this.On = authGrant.On;
                this.Order = authGrant.Order;
                this.IsReject = authGrant.IsReject;
                this.ToName = toName;
                this.CreatorName = creatorName;
            }
        }
    }
}
