using FCloud3.Entities.Messages;
using FCloud3.Repos.Messages;
using FCloud3.Services.Etc.Metadata;
using FCloud3.Services.Identities;

namespace FCloud3.Services.Messages
{
    public class CommentService(
        CommentRepo commentRepo,
        UserService userService,
        UserMetadataService userMetadataService,
        MaterialMetadataService materialMetadataService)
    {
        private readonly CommentRepo _commentRepo = commentRepo;
        private readonly UserService _userService = userService;
        private readonly UserMetadataService _userMetadataService = userMetadataService;
        private readonly MaterialMetadataService _materialMetadataService = materialMetadataService;

        public bool Create(Comment comment, out string? errmsg)
        {
            return _commentRepo.TryAdd(comment, out errmsg);
        }

        public List<CommentViewResult> View(CommentTargetType type, int objId)
        {
            var all = _commentRepo.GetComments(type, objId).OrderBy(x => x.Created).ToList();
            var relatedUsers = all.Select(x => x.CreatorUserId).Distinct().ToList();
            var users = _userMetadataService.GetRange(relatedUsers);
            var relatedMaterials = users.Select(x => x.AvatarMaterialId).Distinct().ToList();
            var materials = _materialMetadataService.GetRange(relatedMaterials);
            var root = new CommentViewResult() { Id = 0 };
            root.PopulateReplies(all, users, materials, _userService.AvatarFullUrl, 0);
            root.Replies.Reverse();
            return root.Replies;
        }

        /// <summary>
        /// 最大缩进级数，可根据需要调整
        /// </summary>
        private const int maxCommentIndent = 2;
        public class CommentViewResult
        {
            public int Id { get; set; }
            public string? Content { get; set; }
            public byte Rate { get; set; }
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? UserAvtSrc { get; set; }
            public string? Time { get; set; }
            /// <summary>
            /// 缩进层级显示时为0，如果被扁平化了则会赋值为其父级Id
            /// </summary>
            public int Replying { get; set; }
            public List<CommentViewResult> Replies { get; set; } = [];
            public void PopulateReplies(
                List<Comment> allc,
                List<UserMetadata> allu,
                List<MaterialMetaData> allm,
                Func<string?, string> avtSrc,
                int layer)
            {
                layer++;
                if (layer > allc.Count)
                    throw new Exception("评论加载异常");
                Replies = allc.FindAll(x => x.ReplyingTo == Id).ConvertAll(x => {
                    var owner = allu.FirstOrDefault(u => u.Id == x.CreatorUserId);
                    var ownerAvtMat = allm.FirstOrDefault(m => m.Id == owner?.AvatarMaterialId);
                    var avt = avtSrc(ownerAvtMat?.PathName);
                    return new CommentViewResult
                    {
                        Id = x.Id,
                        Content = x.Content,
                        Rate = x.Rate,
                        UserId = x.CreatorUserId,
                        UserName = owner?.Name ?? "??",
                        UserAvtSrc = avt,
                        Time = x.Created.ToString("yyyy-MM-dd HH:mm")
                    };
                });
                Replies.ForEach(r => r.PopulateReplies(allc, allu, allm, avtSrc, layer));
                if (layer > maxCommentIndent)
                    Replies = GetChildrenFlatened(allc, true) ?? [];
            }

            /// <summary>
            /// 递归地将内部的后代级评论全部扁平化，
            /// 除了目前层级的直属子级（isMasterCall为true）保持Replying为0之外，<br/>
            /// 其他孙级以下的评论需要被拿出来，并在Replying赋值其父级的id
            /// 前端拿到数据后会自动引用其父级的发送者名称和部分正文
            /// </summary>
            /// <param name="allc">本主题下全部评论</param>
            /// <param name="isMasterCall">是第一次调用（并非递归调用）</param>
            /// <returns></returns>
            public List<CommentViewResult>? GetChildrenFlatened(List<Comment> allc, bool isMasterCall)
            {
                if (this.Replies.Count == 0)
                    return null;
                List<CommentViewResult> res = [];
                Replies.ForEach(r => {
                    if (!isMasterCall)
                        r.Replying = allc.Find(x => x.Id == r.Id)?.ReplyingTo ?? 0; 
                    res.Add(r);
                    var descendents = r.GetChildrenFlatened(allc, false);
                    if(descendents is not null)
                        res.AddRange(descendents);
                });
                Replies.Clear();
                return res;
            }
        }
    }
}
