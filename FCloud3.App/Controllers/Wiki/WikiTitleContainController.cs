using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Wiki
{
    [Authorize]
    public class WikiTitleContainController : Controller
    {
        private readonly WikiTitleContainService _wikiTitleContainService;

        public WikiTitleContainController(WikiTitleContainService wikiTitleContainService)
        {
            _wikiTitleContainService = wikiTitleContainService;
        }
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult GetAll([FromBody]WikiTitleContainGetAllRequest req)
        {
            var resp = _wikiTitleContainService.GetContains(req.Type, req.ObjectId);
            return this.ApiResp(resp);
        }
        [AllowAnonymous]
        public IActionResult GetAllForWiki(int wikiId)
        {
            var resp = _wikiTitleContainService.GetContainsForWiki(wikiId);
            return this.ApiResp(resp);
        }        
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult SetAll([FromBody]WikiTitleContainSetAllRequest req)
        {
            if (req.WikiIds is null)
                return BadRequest();
            _wikiTitleContainService.SetContains(req.Type, req.ObjectId, req.WikiIds);
            return this.ApiResp();
        }
        [UserTypeRestricted]
        public IActionResult AutoFill(int objId, WikiTitleContainType containType, string content)
        {
            var resp = _wikiTitleContainService.AutoFill(objId, containType, content);
            return this.ApiResp(resp);
        }

        public class WikiTitleContainGetAllRequest : IAuthGrantableRequstModelWithOn
        {
            public WikiTitleContainType Type { get; set; }
            public int ObjectId { get; set; }

            public AuthGrantOn AuthGrantOnType
            {
                get
                {
                    if (Type == WikiTitleContainType.TextSection)
                        return AuthGrantOn.TextSection;
                    else if (Type == WikiTitleContainType.FreeTable)
                        return AuthGrantOn.FreeTable;
                    return AuthGrantOn.None;
                }
            }
            public int AuthGrantOnId => ObjectId;
        }
        public class WikiTitleContainSetAllRequest : IAuthGrantableRequstModelWithOn
        {
            public WikiTitleContainType Type { get; set; }
            public int ObjectId { get; set; }
            public List<int>? WikiIds { get; set; }

            public AuthGrantOn AuthGrantOnType
            {
                get
                {
                    if (Type == WikiTitleContainType.TextSection)
                        return AuthGrantOn.TextSection;
                    else if (Type == WikiTitleContainType.FreeTable)
                        return AuthGrantOn.FreeTable;
                    return AuthGrantOn.None;
                }
            }
            public int AuthGrantOnId => ObjectId;
        }
    }
}
