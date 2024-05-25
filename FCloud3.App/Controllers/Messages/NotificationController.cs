using FCloud3.App.Services.Utils;
using FCloud3.Services.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Messages
{
    public class NotificationController(
        NotificationService notificationService,
        HttpUserIdProvider httpUserIdProvider)
        : Controller
    {
        private readonly NotificationService _notificationService = notificationService;
        private readonly HttpUserIdProvider _httpUserIdProvider = httpUserIdProvider;

        [Authorize]
        public IActionResult Get([FromBody]NotificationGetRequest req)
        {
            return this.ApiResp(_notificationService.View(req.Skip));
        }
        public IActionResult Count()
        {
            if (_httpUserIdProvider.Get() == 0)
                return this.ApiResp(0);
            return this.ApiResp(_notificationService.UnreadCount());
        }
        
        [Authorize]
        public IActionResult MarkRead(int id)
        {
            if(id == -1)
                _notificationService.MarkAllRead();
            else
                _notificationService.MarkRead(id);
            return this.ApiResp();
        }

        public class NotificationGetRequest
        {
            public int Skip { get; set; }
        }
    }
}
