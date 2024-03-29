using FCloud3.Entities.Wiki;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Wiki
{
    public class WikiTitleContainController : Controller
    {
        private readonly WikiTitleContainService _wikiTitleContainService;

        public WikiTitleContainController(WikiTitleContainService wikiTitleContainService)
        {
            _wikiTitleContainService = wikiTitleContainService;
        }

        public IActionResult GetAll([FromBody]WikiTitleContainGetAllRequest req)
        {
            var resp = _wikiTitleContainService.GetAll(req.Type, req.ObjectId);
            return this.ApiResp(resp);
        }
        public IActionResult SetAll([FromBody]WikiTitleContainSetAllRequest req)
        {
            if (req.WikiIds is null)
                return BadRequest();
            if(!_wikiTitleContainService.SetAll(req.Type, req.ObjectId, req.WikiIds, out string? errmsg))
                this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult AutoFill(string content)
        {
            var resp = _wikiTitleContainService.AutoFill(content);
            return this.ApiResp(resp);
        }

        public class WikiTitleContainGetAllRequest
        {
            public WikiTitleContainType Type { get; set; }
            public int ObjectId { get; set; }
        }
        public class WikiTitleContainSetAllRequest
        {
            public WikiTitleContainType Type { get; set; }
            public int ObjectId { get; set; }
            public List<int>? WikiIds { get; set; }
        }
    }
}
