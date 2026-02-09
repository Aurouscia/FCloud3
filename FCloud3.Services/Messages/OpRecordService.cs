using FCloud3.Entities.Messages;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Messages
{
    public class OpRecordService(
        OpRecordRepo opRecordRepo,
        UserRepo userRepo,
        WikiParaRepo wikiParaRepo)
    {

        public List<OpRecordViewModel> Get(int skip, int user = -1)
        {
            List<OpRecord> ops;
            if (user == -1)
                ops = opRecordRepo.TakeRange(skip).ToList();
            else
                ops = opRecordRepo.TakeRange(skip, user).ToList();
            return ToViewModels(ops);
        }

        public List<OpRecordViewModel> GetRecordsOfWiki(int wikiId)
        {
            var ops = opRecordRepo.Existing
                .Where(x => x.TargetType == OpRecordTargetType.WikiItem)
                .Where(x => x.ObjA == wikiId)
                .OrderByDescending(x => x.Id)
                .ToList();
            return ToViewModels(ops);
        }

        private List<OpRecordViewModel> ToViewModels(IEnumerable<OpRecord> records)
        {
            return records.Select(op =>
            {
                var u = userRepo.CachedItemById(op.CreatorUserId);
                return new OpRecordViewModel()
                {
                    Id = op.Id,
                    Content = op.Content,
                    OpType = op.OpType,
                    TargetType = op.TargetType,
                    TargetObjId = op.ObjA,
                    OtherObjId = op.ObjB,
                    UserId = op.CreatorUserId,
                    UserName = u?.Name ?? "??",
                    Time = op.Created.ToString("yyyy-MM-dd HH:mm")
                };
            }).ToList();
        }

        public class OpRecordViewModel
        {
            public int Id { get; set; }
            public string? Content { get; set; }
            public OpRecordOpType OpType { get; set; }
            public OpRecordTargetType TargetType { get; set; }
            public int TargetObjId { get; set; }
            public int OtherObjId { get; set; }
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? Time { get; set; }
        }
    }
}
