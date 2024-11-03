using FCloud3.Entities.Identities;
using FCloud3.Entities.Sys;
using FCloud3.Repos.Sys;
using FCloud3.Services.Etc.Cache.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Etc.Cache
{
    public class AuthResCacheHost(LastUpdateRepo lastUpdateRepo)
        : NewestEnsuredCacheHost<bool>(lastUpdateRepo)
    {
        protected override LastUpdateType[] LuTypes => [
            LastUpdateType.AuthGrant, LastUpdateType.UserToGroup, LastUpdateType.UserGroup];
        private static string CacheKey(AuthGrantOn on, int onId, int userId)
            => $"authres_{(int)on}_{onId}_{userId}";
        public void SetCache(AuthGrantOn on, int onId, int userId, bool canAccess)
        {
            string cacheKey = CacheKey(on, onId, userId);
            base.Set(cacheKey, canAccess);
        }
        public bool TryReadCache(AuthGrantOn on, int onId, int userId, out bool canAccess)
        {
            canAccess = false;
            string cacheKey = CacheKey(on, onId, userId);
            if (base.TryGet(cacheKey, out bool res))
            {
                canAccess = res;
                return true;
            }
            return false;
        }
    }
}
