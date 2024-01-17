using FCloud3.Repos;
using FCloud3.Services.Files;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Files
{
    public class FileDirController: Controller
    {
        private readonly FileDirService _fileDirService;

        public FileDirController(FileDirService fileDirService)
        {
            _fileDirService = fileDirService;
        }

        public IActionResult Index([FromBody]FileDirIndexRequest req)
        {
            if (req.Query is null || req.Path is null)
                return BadRequest();
            var res = _fileDirService.GetListByPath(req.Query, req.Path,out string? errmsg);
            if(res is not null)
                return this.ApiResp(res);
            return this.ApiFailedResp(errmsg);
        }
        public IActionResult TakeContent(int dirId)
        {
            return this.ApiResp(_fileDirService.TakeContent(dirId));
        }
        public class FileDirIndexRequest
        {
            public string[]? Path { get; set; }
            public IndexQuery? Query { get; set; }
        }

    }
}
