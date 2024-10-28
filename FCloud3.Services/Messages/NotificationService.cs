using FCloud3.Entities.Messages;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Messages
{
    public class NotificationService(
        NotificationRepo notificationRepo,
        CommentRepo commentRepo,
        UserGroupRepo userGroupRepo,
        UserRepo userRepo,
        WikiItemRepo wikiItemRepo,
        IOperatingUserIdProvider operatingUserIdProvider
        )
    {
        public NotifViewResult View(int skip)
        {
            var toView = notificationRepo.TakeRange(skip, 20).ToList();
            var items = GetViewItem(toView);
            var count = notificationRepo.Mine.Count();
            return new(items, count);
        }
        private List<NotifViewItem> GetViewItem(List<Notification> models)
        {
            var relatedCommentIds = models
                .Where(x => x.Type == NotifType.CommentWiki || x.Type == NotifType.CommentWikiReply)
                .Select(x => x.Param2).ToList();
            var relatedComments = commentRepo.GetRangeByIds(relatedCommentIds).ToList();
            var relatedGroupIds = models
                .Where(x => x.Type == NotifType.UserGroupInvite)
                .Select(x => x.Param1) //param1就是groupId
                .ToList();
            var relatedGroups = userGroupRepo
                .GetRangeByIds(relatedGroupIds)
                .Select(x => new { x.Id, x.Name }).ToList();

            return models.ConvertAll(x =>
            {
                var senderId = x.Sender;
                var senderName = userRepo.CachedItemById(x.Sender)?.Name ?? "";
                if(x.Type == NotifType.CommentWiki || x.Type == NotifType.CommentWikiReply)
                {
                    var wikiId = x.Param1;
                    var wikiTitle = wikiItemRepo.CachedItemById(x.Param1)?.Title ?? "";
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
                    var groupName = relatedGroups.Find(x => x.Id==groupId)?.Name ?? "";
                    return new NotifViewItem(x.Id, x.Read, x.Created, x.Type, senderId, senderName, groupId, groupName);
                }
                throw new NotImplementedException();
            });
        }
        public int UnreadCount() => notificationRepo.MineUnread.Count();
        public void MarkRead(int id) => notificationRepo.MarkRead(id);
        public void MarkAllRead() => notificationRepo.MarkAllRead();


        public void CommentWiki(int wikiId, int commentId, int wikiOwner = 0)
        {
            if (wikiOwner == 0)
            {
                var wiki = wikiItemRepo.CachedItemById(wikiId) ?? throw new Exception("找不到指定词条");
                wikiOwner = wiki.OwnerId;
            }
            var sender = operatingUserIdProvider.Get();
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
            notificationRepo.Add(notif);
        }
        public void CommentWikiReply(int wikiId, int commentId, int replyTo)
        {
            var sender = operatingUserIdProvider.Get();
            var targetCmtOwner = commentRepo.Existing.Where(x=>x.Id == replyTo).Select(x=>x.CreatorUserId).FirstOrDefault();
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
            notificationRepo.Add(notif);
        }
        public void UserGroupInvite(int groupId, int invitedUser)
        {
            var sender = operatingUserIdProvider.Get();
            Notification notif = new()
            {
                Sender = sender,
                Type = NotifType.UserGroupInvite,
                Receiver = invitedUser,
                Param1 = groupId,
                Param2 = 0,
                Read = false
            };
            notificationRepo.Add(notif);
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
