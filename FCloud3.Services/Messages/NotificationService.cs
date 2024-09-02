using FCloud3.Entities.Messages;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.Identities;

namespace FCloud3.Services.Messages
{
    public class NotificationService(
        NotificationRepo notificationRepo,
        IOperatingUserIdProvider operatingUserIdProvider,
        WikiItemCaching wikiItemCaching,
        UserCaching userCaching,
        CommentRepo commentRepo,
        UserGroupRepo userGroupRepo
        )
    {
        private readonly NotificationRepo _notificationRepo = notificationRepo;
        private readonly IOperatingUserIdProvider _operatingUserIdProvider = operatingUserIdProvider;
        private readonly WikiItemCaching _wikiItemCaching = wikiItemCaching;
        private readonly UserCaching _userCaching = userCaching;
        private readonly UserGroupRepo _userGroupRepo = userGroupRepo;
        private readonly CommentRepo _commentRepo = commentRepo;

        public NotifViewResult View(int skip)
        {
            var toView = _notificationRepo.TakeRange(skip, 20).ToList();
            var items = GetViewItem(toView);
            var count = _notificationRepo.Mine.Count();
            return new(items, count);
        }
        private List<NotifViewItem> GetViewItem(List<Notification> models)
        {
            var relatedCommentIds = models
                .Where(x => x.Type == NotifType.CommentWiki || x.Type == NotifType.CommentWikiReply)
                .Select(x => x.Param2).ToList();
            var relatedComments = _commentRepo.GetRangeByIds(relatedCommentIds).ToList();

            return models.ConvertAll(x =>
            {
                var senderId = x.Sender;
                var senderName = _userCaching.Get(x.Sender)?.Name ?? "";
                if(x.Type == NotifType.CommentWiki || x.Type == NotifType.CommentWikiReply)
                {
                    var wikiId = x.Param1;
                    var wikiTitle = _wikiItemCaching.Get(x.Param1)?.Title ?? "";
                    var cmtId = x.Param2;
                    string cmtBrief;
                    var cmt = relatedComments.Find(c => c.Id == cmtId);
                    if (cmt is null || cmt.IsHidden())
                        cmtBrief = "<该评论已被删除>";
                    else
                    {
                        cmtBrief = cmt.Content ?? "";
                        if (cmtBrief.Length > 20)
                            cmtBrief = cmtBrief[..20] + "...";
                    }
                    return new NotifViewItem(x.Id, x.Read, x.Created, x.Type, senderId, senderName, wikiId, wikiTitle, cmtId, cmtBrief);
                }
                if(x.Type == NotifType.UserGroupInvite)
                {
                    var groupId = x.Param1;
                    var groupName = _userGroupRepo.GetById(groupId)?.Name ?? "";
                    return new NotifViewItem(x.Id, x.Read, x.Created, x.Type, senderId, senderName, groupId, groupName);
                }
                throw new NotImplementedException();
            });
        }
        public int UnreadCount() => _notificationRepo.MineUnread.Count();
        public void MarkRead(int id) => _notificationRepo.MarkRead(id);
        public void MarkAllRead() => _notificationRepo.MarkAllRead();


        public void CommentWiki(int wikiId, int commentId, int wikiOwner = 0)
        {
            if (wikiOwner == 0)
            {
                var wiki = _wikiItemCaching.Get(wikiId) ?? throw new Exception("找不到指定词条");
                wikiOwner = wiki.OwnerId;
            }
            var sender = _operatingUserIdProvider.Get();
            if (sender == wikiOwner)
                return;
            Notification notif = new()
            {
                Sender = sender,
                Type = NotifType.CommentWiki,
                Receiver = wikiOwner,
                Param1 = wikiId,
                Param2 = commentId,
                Read = false
            };
            _ = _notificationRepo.TryAdd(notif, out _);
        }
        public void CommentWikiReply(int wikiId, int commentId, int replyTo)
        {
            var sender = _operatingUserIdProvider.Get();
            var targetCmtOwner = _commentRepo.Existing.Where(x=>x.Id == replyTo).Select(x=>x.CreatorUserId).FirstOrDefault();
            if (sender == targetCmtOwner)
                return;
            Notification notif = new()
            {
                Sender = sender,
                Type = NotifType.CommentWikiReply,
                Receiver = targetCmtOwner,
                Param1 = wikiId,
                Param2 = commentId,
                Read = false
            };
            _ = _notificationRepo.TryAdd(notif, out _);
        }
        public void UserGroupInvite(int groupId, int invitedUser)
        {
            var sender = _operatingUserIdProvider.Get();
            Notification notif = new()
            {
                Sender = sender,
                Type = NotifType.UserGroupInvite,
                Receiver = invitedUser,
                Param1 = groupId,
                Param2 = 0,
                Read = false
            };
            _ = _notificationRepo.TryAdd(notif, out _);
        }


        public class NotifViewResult(List<NotifViewItem> items, int totalCount)
        {
            public List<NotifViewItem> Items { get; set; } = items;
            public int TotalCount { get; set; } = totalCount;
        }
        public class NotifViewItem(
            int id, bool read, DateTime time, NotifType type, int senderId, string? senderName,
            int param1, string? param1Text, int param2 = 0, string? param2Text = "")
        {
            public int Id { get; set; } = id;
            public bool Read { get; set; } = read;
            public string Time { get; set; } = time.ToString("yyyy-MM-dd HH:mm");
            public NotifType Type { get; set; } = type;
            public int SId { get; set; } = senderId;
            public string? SName { get; set; } = senderName;
            public int P1 { get; set; } = param1;
            public string? P1T { get; set; } = param1Text;
            public int P2 { get; set; } = param2;
            public string? P2T { get; set; } = param2Text;
        }
    }
}
