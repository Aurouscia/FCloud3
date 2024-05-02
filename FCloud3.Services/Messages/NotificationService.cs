using FCloud3.Entities.Messages;
using FCloud3.Repos.Messages;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Etc.Metadata;

namespace FCloud3.Services.Messages
{
    public class NotificationService(
        NotificationRepo notificationRepo,
        IOperatingUserIdProvider operatingUserIdProvider,
        WikiItemMetadataService wikiItemMetadataService,
        UserMetadataService userMetadataService
        )
    {
        private readonly NotificationRepo _notificationRepo = notificationRepo;
        private readonly IOperatingUserIdProvider _operatingUserIdProvider = operatingUserIdProvider;
        private readonly WikiItemMetadataService _wikiItemMetadataService = wikiItemMetadataService;
        private readonly UserMetadataService _userMetadataService = userMetadataService;

        public List<NotifViewItem> View(int skip)
        {
            var toView = _notificationRepo.TakeRange(skip, 20).ToList();
            return GetViewItem(toView);
        }
        private List<NotifViewItem> GetViewItem(List<Notification> models)
        {
            return models.ConvertAll(x =>
            {
                var senderId = x.Sender;
                var senderName = _userMetadataService.Get(x.Sender)?.Name ?? "";
                if(x.Type == NotifType.CommentMyWiki)
                {
                    var wikiId = x.Param1;
                    var wikiTitle = _wikiItemMetadataService.Get(x.Param1)?.Title ?? "";
                    return new NotifViewItem(x.Id, x.Type, senderId, senderName, wikiId, wikiTitle);
                }
                throw new NotImplementedException();
            });
        }
        public void CommentWiki(int wikiId, int wikiOwner = 0)
        {
            if (wikiOwner == 0)
            {
                var wiki = _wikiItemMetadataService.Get(wikiId) ?? throw new Exception("找不到指定词条");
                wikiOwner = wiki.OwnerId;
            }
            var sender = _operatingUserIdProvider.Get();
            Notification notif = new()
            {
                Sender = sender,
                Receiver = wikiOwner,
                Param1 = wikiId,
                Param2 = 0,
                Read = false
            };
            _ = _notificationRepo.TryAdd(notif, out _);
        }


        public class NotifViewItem(
            int id, NotifType type, int senderId, string? senderName,
            int param1, string? param1Text, int param2 = 0, string? param2Text = "")
        {
            public int Id { get; set; } = id;
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
