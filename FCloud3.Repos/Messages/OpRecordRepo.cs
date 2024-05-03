using FCloud3.DbContexts;
using FCloud3.Entities.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Messages
{
    public class OpRecordRepo : RepoBase<OpRecord>
    {
        public OpRecordRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public void Record(OpRecordOpType opType, OpRecordTargetType targetType, string? content = null)
        {
            OpRecord r = new()
            {
                OpType = opType,
                TargetType = targetType,
                Content = content,
            };
            _ = TryAdd(r, out _);
        }
    }
}
