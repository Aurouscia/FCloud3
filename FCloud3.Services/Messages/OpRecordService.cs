using FCloud3.Entities.Messages;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Etc.Caching;

namespace FCloud3.Services.Messages
{
    public class OpRecordService(
        OpRecordRepo opRecordRepo,
        UserCaching userCaching)
    {
        private readonly OpRecordRepo _opRecordRepo = opRecordRepo;
        private readonly UserCaching _userCaching = userCaching;

        public List<OpRecordViewModel> Get(int skip, int user = -1)
        {
            List<OpRecord> ops;
            if (user == -1)
                ops = _opRecordRepo.TakeRange(skip).ToList();
            else
                ops = _opRecordRepo.TakeRange(skip, user).ToList();
            var userIds = ops.ConvertAll(x => x.CreatorUserId);
            var users = _userCaching.GetRange(userIds);
            return ops.ConvertAll(op =>
            {
                var u = users.Find(x => x.Id == op.CreatorUserId);
                return new OpRecordViewModel()
                {
                    Id = op.Id,
                    Content = op.Content,
                    OpType = op.OpType,
                    TargetType = op.TargetType,
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
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? Time { get; set; }
        }
    }
}
