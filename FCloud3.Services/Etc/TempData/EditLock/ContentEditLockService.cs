using FCloud3.Repos.Identities;
using FCloud3.Services.Etc.TempData.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCloud3.Services.Etc.TempData.EditLock
{
    public class ContentEditLockService
    {
        private readonly TempDataContext _context;
        private readonly UserRepo _userRepo;
        private readonly IOperatingUserIdProvider _userIdProvider;
        private readonly static Random _random = new();
        private readonly ILogger<ContentEditLockService> _logger;

        private const int lockDiscardSeconds = 180;
        private const int cleanupProb = 30;
        private const int cleanupThrsSeconds = 24 * 60 * 60;

        public ContentEditLockService(
            TempDataContext context,
            UserRepo userRepo,
            IOperatingUserIdProvider userIdProvider,
            ILogger<ContentEditLockService> logger) 
        {
            _context = context;
            _userRepo = userRepo;
            _userIdProvider = userIdProvider;
            _logger = logger;
        }

        public bool HeartbeatRange(List<(HeartbeatObjType type, int objId)> targets, bool isInitial, out string? errmsg)
        {
            foreach(var tar in targets)
            {
                var s = Heartbeat(tar.type, tar.objId, isInitial, out errmsg);
                if (!s)
                {
                    return false;
                }
            }
            errmsg = null;
            return true;
        }
       
        public bool Heartbeat(HeartbeatObjType type, int objId, bool isInitial, out string? errmsg)
        {
            if (type == HeartbeatObjType.None)
            {
                errmsg = "类型未知";
                return false;
            }
            int userId = _userIdProvider.Get();
            var model = _context.ContentEditLock.Where(x => x.ObjectType == type && x.ObjectId == objId).FirstOrDefault();
            var now = TimeStamp();
            if (model is not null)
            {
                if(model.UserId != userId)
                {
                    if (!isInitial)
                    {
                        //如果是续约，那就必须是自己开的，否则说明中间中断的时间被人写过
                        var occupyUserName = _userRepo.GetNameById(model.UserId, 6) ?? "??";
                        errmsg = $"该内容已被 [{occupyUserName}] 编辑过，请手动保存更改到别处并刷新";
                        _logger.LogDebug("u_{uid}-心跳续约失败-{type}-{objId}", userId, type, objId);
                        return false;
                    }
                    if (now - model.TimeStamp < lockDiscardSeconds)
                    {
                        //其他用户在 lockDiscardSeconds 内报告过，阻止本次报告，并返回该用户id
                        var occupyUserName = _userRepo.GetNameById(model.UserId, 6) ?? "??";
                        errmsg = $"该内容正在被 [{occupyUserName}] 编辑，请退出，稍后再尝试进入";
                        _logger.LogDebug("u_{uid}-心跳启动失败-{type}-{objId}", userId, type, objId);
                        return false;
                    }
                }
                model.UserId = userId;
                model.TimeStamp = now;
            }
            else
            {
                model = new()
                {
                    UserId = userId,
                    TimeStamp = TimeStamp(),
                    ObjectId = objId,
                    ObjectType = type
                };
                _context.Add(model);
            }
            _context.SaveChanges();

            if (_random.Next(0, cleanupProb) == 0)
                Shrink();

            errmsg = null;
            if(isInitial)
                _logger.LogDebug("u_{uid}-心跳启动成功-{type}-{objId}", userId, type, objId);
            else
                _logger.LogDebug("u_{uid}-心跳续约成功-{type}-{objId}", userId, type, objId);
            return true;
        }
       
        public void Shrink()
        {
            long now = TimeStamp();
            long cleanup = now - cleanupThrsSeconds;
            _context.ContentEditLock.Where(x => x.TimeStamp < cleanup).ExecuteDelete();
        }

        private static long TimeStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
