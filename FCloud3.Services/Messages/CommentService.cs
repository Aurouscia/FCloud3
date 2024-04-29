using FCloud3.Entities.Messages;
using FCloud3.Repos.Messages;
using FCloud3.Services.Etc.Metadata;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Identities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var all = _commentRepo.GetComments(type, objId).OrderByDescending(x => x.Created).ToList();
            var relatedUsers = all.Select(x => x.CreatorUserId).Distinct().ToList();
            var users = _userMetadataService.GetRange(relatedUsers);
            var relatedMaterials = users.Select(x => x.AvatarMaterialId).Distinct().ToList();
            var materials = _materialMetadataService.GetRange(relatedMaterials);
            var root = new CommentViewResult() { Id = 0 };
            root.PopulateReplies(all, users, materials, _userService.AvatarFullUrl, 0);
            return root.Replies;
        }

        public class CommentViewResult
        {
            public int Id { get; set; }
            public string? Content { get; set; }
            public byte Rate { get; set; }
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? UserAvtSrc { get; set; }
            public string? Time { get; set; }
            public List<CommentViewResult> Replies { get; set; } = [];
            public void PopulateReplies(
                List<Comment> allc,
                List<UserMetadata> allu,
                List<MaterialMetaData> allm,
                Func<string?, string> avtSrc,
                int safeVal)
            {
                if (++safeVal > 10)
                    throw new Exception("评论加载异常：过深递归");
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
                Replies.ForEach(r => r.PopulateReplies(allc, allu, allm, avtSrc, safeVal));
            }
        }
    }
}
