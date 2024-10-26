using FCloud3.Entities.Messages;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Messages;

namespace FCloud3.Services.Messages
{
    public class OpRecordService(
        OpRecordRepo opRecordRepo,
        UserRepo userRepo)
    {

        public List<OpRecordViewModel> Get(int skip, int user = -1)
        {
            List<OpRecord> ops;
            if (user == -1)
                ops = opRecordRepo.TakeRange(skip).ToList();
            else
                ops = opRecordRepo.TakeRange(skip, user).ToList();
            var userIds = ops.ConvertAll(x => x.CreatorUserId);
            return ops.ConvertAll(op =>
            {
                var u = userRepo.CachedItemById(op.CreatorUserId);
                return new OpRecordViewModel()
                {
                    Id = op.Id,
                    Content = op.Content,
                    OpType = op.OpType,
                    TargetType = op.TargetType,
                    TargetObjId = op.ObjA,
                    UserId = op.CreatorUserId,
                    UserName = u?.Name ?? "??",
                    Time = op.Created.ToString("yyyy-MM-dd HH:mm")
                };
            });
        }

        public class OpRecordViewModel
        {
            public int Id { get; set; }
            public string? Content { get; set; }
            public OpRecordOpType OpType { get; set; }
            public OpRecordTargetType TargetType { get; set; }
            public int TargetObjId { get; set; }
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? Time { get; set; }
        }
    }
}
