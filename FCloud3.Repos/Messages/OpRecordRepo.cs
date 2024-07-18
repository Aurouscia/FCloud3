using FCloud3.DbContexts;
using FCloud3.Entities.Messages;
using FCloud3.Repos.Etc;
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

        public void Record(OpRecordOpType opType, OpRecordTargetType targetType, int objA, int objB, string? content = null)
        {
            OpRecord r = new()
            {
                OpType = opType,
                TargetType = targetType,
                Content = content,
                ObjA = objA,
                ObjB = objB
            };
            _ = TryAdd(r, out _);
        }

        public IQueryable<OpRecord> TakeRange(int skip) 
            => Existing.OrderByDescending(x => x.Created).Skip(skip).Take(20);
        public IQueryable<OpRecord> TakeRange(int skip, int user)
            => Existing.Where(x => x.CreatorUserId == user).OrderByDescending(x => x.Created).Skip(skip).Take(20);
    }
}
