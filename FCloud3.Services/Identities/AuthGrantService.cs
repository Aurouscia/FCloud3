﻿using FCloud3.Entities;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc;
using FCloud3.Services.Etc.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.Services.Identities
{
    public class AuthGrantService(
        AuthGrantRepo authGrantRepo,
        UserRepo userRepo,
        UserGroupRepo userGroupRepo,
        UserToGroupRepo userToGroupRepo,
        WikiItemRepo wikiItemRepo,
        WikiParaRepo wikiParaRepo,
        FileDirRepo fileDirRepo,
        FileItemRepo fileItemRepo,
        MaterialRepo materialRepo,
        IOperatingUserIdProvider userIdProvider,
        AuthResCacheHost authResCacheHost)
    {

        public int TestedCount { get; private set; }
        
        public bool CheckAccess(AuthGrantOn on, int onId)
        {
            int userId = userIdProvider.Get();
            if (authResCacheHost.TryReadCache(on, onId, userId, out bool canAccess))
            {
                return canAccess;
            }
            var queried = CheckAccessWithoutCache(on, onId);
            authResCacheHost.SetCache(on, onId, userId, queried);
            TestedCount++;
            return queried;
        }
        private bool CheckAccessWithoutCache(AuthGrantOn on, int onId)
        {
            int userId = userIdProvider.Get();
            if (userId == 0)
                return false;

            Redirect(on, onId, out on, out onId);

            if (on == AuthGrantOn.None)
                return false;
            var ownerId = GetOwnerId(on, onId);
            if (userId == ownerId)
                return true;//如果所有者就是访问者，直接提供
            else
            {
                if(OwnerOnly(on))
                    return false;//如果所有者不是访问者，但是该类型只允许所有者访问，直接拒绝
            }

            var gs = authGrantRepo.GetByOn(on, onId, ownerId);
            gs.Reverse();//下面覆盖上面，所以先检验

            if(GetBuiltInOfInCacheModel(on) is List<AuthGrantCacheModel> baseAuths){
                gs.AddRange(baseAuths);//添加该类型的系统默认权限在队尾
            }

            var groupIds = gs.Where(x => x.To == AuthGrantTo.UserGroup).Select(x => x.ToId).ToList();
            var groupDict = userToGroupRepo.GetMembersDict(groupIds);

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
                if (g.To == AuthGrantTo.SameGroup)
                {
                    if(userToGroupRepo.IsInSameGroup(userId, ownerId))
                        return !g.IsReject;
                }
            }
            return ownerId == userId;
        }
        /// <summary>
        /// TODO：目前不支持一个段落存在于多个词条中，段落的权限验证转换为其词条的权限验证
        /// </summary>
        /// <param name="on"></param>
        /// <param name="onId"></param>
        /// <param name="toOn"></param>
        /// <param name="toOnId"></param>
        private void Redirect(AuthGrantOn on, int onId, out AuthGrantOn toOn, out int toOnId)
        {
            toOn = on;
            toOnId = onId;
            bool problematic = false;
            if(on == AuthGrantOn.TextSection)
            {
                var wikiIds = wikiParaRepo.WithType(WikiParaType.Text).Where(x => x.ObjectId == onId).Select(x => x.WikiItemId).ToList();
                if (wikiIds.Count != 1)
                    problematic = true;
                else
                {
                    toOnId = wikiIds[0];
                    toOn = AuthGrantOn.WikiItem;
                }
            }
            else if(on == AuthGrantOn.FreeTable)
            {
                var wikiIds = wikiParaRepo.WithType(WikiParaType.Table).Where(x => x.ObjectId == onId).Select(x => x.WikiItemId).ToList();
                if (wikiIds.Count != 1)
                    problematic = true;
                else
                {
                    toOnId = wikiIds[0];
                    toOn = AuthGrantOn.WikiItem;
                }
            }
            else if(on == AuthGrantOn.WikiPara)
            {
                var para = wikiParaRepo.GetById(onId);
                if (para is not null)
                {
                    toOnId = para.WikiItemId;
                    toOn = AuthGrantOn.WikiItem;
                }
                else
                    problematic = true;
            }
            if (problematic)
            {
                toOn = AuthGrantOn.None;
                toOnId = 0;
            }
        }

        private List<AuthGrantOn> OwnerOnlyOnTypes = [AuthGrantOn.User, AuthGrantOn.UserGroup, AuthGrantOn.None];
        private bool OwnerOnly(AuthGrantOn on) => OwnerOnlyOnTypes.Contains(on);
        public List<AuthGrant>? GetBuiltInOf(AuthGrantOn on)
        {
            if (disableBuiltIn)
                return null;
            if (on == AuthGrantOn.WikiItem)
                return [new (){
                    On = AuthGrantOn.WikiItem,
                    OnId = AuthGrant.onIdForAll,
                    To = AuthGrantTo.SameGroup
                }];
            if (on == AuthGrantOn.Dir)
                return [new () {
                    On = AuthGrantOn.Dir,
                    OnId = AuthGrant.onIdForAll,
                    To = AuthGrantTo.SameGroup
                }];
            return null;
        }
        public List<AuthGrantCacheModel>? GetBuiltInOfInCacheModel(AuthGrantOn on)
        {
            var builtins = GetBuiltInOf(on);
            if (builtins is null)
                return null;
            return builtins.ConvertAll(x => new AuthGrantCacheModel(x.Id, x.Updated,
                    x.Order, x.CreatorUserId, x.OnId, x.On, x.ToId, x.To, x.IsReject));
        }


        public AuthGrantViewModel GetList(AuthGrantOn on, int onId)
        {
            var list = new List<AuthGrantCacheModel>();
            var builtIn = GetBuiltInOfInCacheModel(on) ?? [];
            list.AddRange(builtIn);
            int owner = GetOwnerId(on, onId);
            var userDefined = authGrantRepo.GetByOn(on, onId, owner);
            var globalDefined = new List<AuthGrantCacheModel>();
            var localDefined = new List<AuthGrantCacheModel>();
            if (onId != AuthGrant.onIdForAll)
            {
                userDefined.ForEach(x =>
                {
                    if (x.OnId == AuthGrant.onIdForAll)
                        globalDefined.Add(x);
                    else
                        localDefined.Add(x);
                });
            }
            else
            {
                localDefined.AddRange(userDefined);
            }
            list.AddRange(globalDefined);
            list.AddRange(localDefined);
            var groupIds = list.Where(x => x.To == AuthGrantTo.UserGroup).Select(x=>x.ToId).ToList();
            var userIds = list.Where(x => x.To == AuthGrantTo.User).Select(x => x.ToId).ToList();
            var creatorIds = list.Select(x => x.CreatorUserId).ToList();
            userIds = userIds.Union(creatorIds).ToList();
            userIds.RemoveAll(x => x == 0);
            var groupNames = userGroupRepo.GetRangeByIds(groupIds).Select(x => new { x.Id, x.Name}).ToList();
            Func<AuthGrantCacheModel, AuthGrantViewModelItem> convert = x =>
            {
                string? toName = null;
                if (x.To == AuthGrantTo.UserGroup)
                    toName = groupNames.FirstOrDefault(g => g.Id == x.ToId)?.Name;
                else if (x.To == AuthGrantTo.User)
                    toName = userRepo.CachedItemById(x.ToId)?.Name;
                else if (x.To == AuthGrantTo.EveryOne)
                    toName = "所有人";
                else if (x.To == AuthGrantTo.SameGroup)
                    toName = "同组用户";
                toName ??= "N/A";
                string creatorName = userRepo.CachedItemById(x.CreatorUserId)?.Name ?? "N/A";
                return new AuthGrantViewModelItem(x, toName, creatorName);
            };

            AuthGrantViewModel model = new();
            builtIn.ForEach(x => model.BuiltIn.Add(convert(x)));
            globalDefined.ForEach(x => model.Global.Add(convert(x)));
            localDefined.ForEach(x => model.Local.Add(convert(x)));
            return model;
        }
        public bool Add(AuthGrant newGrant, out string? errmsg)
        {
            if (OwnerOnly(newGrant.On))
            {
                errmsg = "该权限类型不允许设置";
                return false;
            }
            int userId = userIdProvider.Get();
            if (!CanEdit(newGrant.On, newGrant.OnId))
            {
                errmsg = "只有所有者能设置权限";
                return false;
            }
            if(newGrant.ToId == userId && newGrant.To == AuthGrantTo.User)
            {
                errmsg = "无需为自己设置权限";
                return false;
            }
            var s = authGrantRepo.TryAdd(newGrant, out errmsg);
            return s;
        }
        public bool Remove(int id, out string? errmsg)
        {
            AuthGrant? target = authGrantRepo.GetById(id);
            if (target is null)
            {
                errmsg = "找不到指定目标，请刷新后重试";
                return false;
            }
            if (!CanEdit(target.On, target.OnId))
            {
                errmsg = "只有所有者能设置权限";
                return false;
            }
            var s = authGrantRepo.TryRemove(target, out errmsg);
            return s;
        }
        public bool SetOrder(AuthGrantOn on, int onId, List<int> ids, out string? errmsg)
        {
            if (ids.Count == 0) {
                errmsg = null;
                return true;
            }
            if (!CanEdit(on, onId))
            {
                errmsg = "只有所有者能设置权限";
                return false;
            }
            if (ids.Count > AuthGrant.maxCountOnSameOn)
            {
                errmsg = "数量超出上限";
                return false;
            }
            var gs = authGrantRepo.GetRangeByIds(ids).ToList();
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
            authGrantRepo.UpdateRangeWithoutCheck(gs);
            errmsg = null;
            return true;
        }

        private bool CanEdit(AuthGrantOn on, int onId)
        {
            if (onId == AuthGrant.onIdForAll)
                return true; // 表示正在设置全局设置，所有者肯定是自己
            int userId = userIdProvider.Get();
            int owner = GetOwnerId(on, onId);
            return userId == owner;
        }

        private int GetOwnerId(AuthGrantOn on, int onId)
        {
            if (on == AuthGrantOn.WikiItem)
            {
                return wikiItemRepo.GetOwnerIdById(onId);
            }
            else if (on == AuthGrantOn.Dir)
            {
                return fileDirRepo.GetOwnerIdById(onId);
            }
            else if (on == AuthGrantOn.Material)
            {
                return materialRepo.GetOwnerIdById(onId);
            }
            else if (on == AuthGrantOn.User)
            {
                return onId; // 我，就是自己的主人！
            }
            else if (on == AuthGrantOn.FileItem)
            {
                return fileItemRepo.GetOwnerIdById(onId);
            }
            else if (on == AuthGrantOn.UserGroup)
            {
                return userGroupRepo.GetOwnerIdById(onId);
            }
            throw new Exception("获取所有者失败");
        }

        private bool disableBuiltIn = false;
        public void DisableBuiltIn()
        {
            disableBuiltIn = true;
        }
        public class AuthGrantViewModel
        {
            public List<AuthGrantViewModelItem> BuiltIn { get; } = [];
            public List<AuthGrantViewModelItem> Global { get; } = [];
            public List<AuthGrantViewModelItem> Local { get; } = [];
        }
        public class AuthGrantViewModelItem:AuthGrant
        {
            public string ToName { get; }
            public string CreatorName { get; }
            public AuthGrantViewModelItem(AuthGrantCacheModel authGrant, string toName, string creatorName)
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
