using FCloud3.App.Services.Filters;
using FCloud3.Entities.Messages;
using FCloud3.Services.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Messages
{
    public class CommentController : Controller
    {
        private readonly CommentService _commentService;
        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [UserTypeRestricted]
        [UserActiveOperation]
        public IActionResult Create([FromBody] Comment comment)
        {
            if(!_commentService.Create(comment, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult View(CommentTargetType type, int objId)
        {
            return this.ApiResp(_commentService.View(type, objId));
        }
    }
}
