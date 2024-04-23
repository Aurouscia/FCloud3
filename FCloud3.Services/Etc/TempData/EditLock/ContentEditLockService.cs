using FCloud3.Repos.Identities;
using FCloud3.Services.Etc.TempData.Context;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Services.Etc.TempData.EditLock
{
    public class ContentEditLockService
    {
        private readonly TempDataContext _context;
        private readonly UserRepo _userRepo;
        private readonly IOperatingUserIdProvider _userIdProvider;
        private readonly static Random _random = new();
        
        private const int lockDiscardSeconds = 180;
        private const int cleanupProb = 30;
        private const int cleanupThrsSeconds = 24 * 60 * 60;

        public ContentEditLockService(TempDataContext context, UserRepo userRepo, IOperatingUserIdProvider userIdProvider) 
        {
            _context = context;
            _userRepo = userRepo;
            _userIdProvider = userIdProvider;
        }

       
        public bool Heartbeat(ObjectType type, int objId, out string? errmsg)
        {
            if (type == ObjectType.None)
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
                    if (now - model.TimeStamp < lockDiscardSeconds)
                    {
                        //其他用户在 lockDiscardSeconds 内报告过，阻止本次报告，并返回该用户id
                        var occupyUserName = _userRepo.GetNameById(model.UserId, 6) ?? "??";
                        errmsg = $"该内容正在被 [{occupyUserName}] 编辑，请退出，稍后再尝试进入";
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
