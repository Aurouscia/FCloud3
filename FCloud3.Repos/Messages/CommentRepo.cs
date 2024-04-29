using FCloud3.DbContexts;
using FCloud3.Entities.Messages;

namespace FCloud3.Repos.Messages
{
    public class CommentRepo : RepoBase<Comment>
    {
        public CommentRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public IQueryable<Comment> GetComments(CommentTargetType type, int targetObjId)
            => Existing.Where(x => x.TargetType == type && targetObjId == x.TargetObjId);

        public override bool TryAddCheck(Comment item, out string? errmsg)
        {
            if(string.IsNullOrWhiteSpace(item.Content))
            {
                errmsg = "评论内容不能为空";
                return false;
            }
            if(item.Content.Length > Comment.contentMaxLength)
            {
                errmsg = $"评论长度不应超过{Comment.contentMaxLength}字";
                return false;
            }
            errmsg = null;
            return true;
        }
    }
}
