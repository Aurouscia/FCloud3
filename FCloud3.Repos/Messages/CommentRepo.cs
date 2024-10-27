using FCloud3.DbContexts;
using FCloud3.Entities.Messages;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Messages
{
    public class CommentRepo : RepoBase<Comment>
    {
        public CommentRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public IQueryable<Comment> GetComments(CommentTargetType type, int targetObjId)
            => Existing.Where(x => x.TargetType == type && targetObjId == x.TargetObjId);

        public bool HideComment(Comment cmt, out string? errmsg)
        {
            cmt.HiddenByUser = _userIdProvider.Get();
            base.Update(cmt);
            errmsg = null;
            return true;
        }

        public bool TryAdd(Comment item, out string? errmsg)
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
            base.Add(item);
            errmsg = null;
            return true;
        }
    }

    public static class CommentExtension
    {
        public static bool IsHidden(this Comment cmt)
        {
            return cmt.HiddenByUser > 0;
        }
    }
}
