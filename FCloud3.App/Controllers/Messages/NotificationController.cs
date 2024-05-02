using FCloud3.Services.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Messages
{
    [Authorize]
    public class NotificationController
        (NotificationService notificationService)
        : Controller
    {
        private readonly NotificationService _notificationService = notificationService;
        public IActionResult Get([FromBody]NotificationGetRequest req)
        {
            return this.ApiResp(_notificationService.View(req.Skip));
        }
        public IActionResult Count()
        {
            return this.ApiResp(_notificationService.UnreadCount());
        }
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
