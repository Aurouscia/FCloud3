using FCloud3.Entities.Files;
using FCloud3.Repos;
using FCloud3.Services.Files;
using Microsoft.AspNetCore.Mvc;
using static FCloud3.Services.Files.FileDirTakeContentResult;

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
            var res = _fileDirService.GetSubDirAndItemsByPath(req.Query, req.Path,out string? errmsg);
            if (res is not null)
            {
                return this.ApiResp(res);
            }
            return this.ApiFailedResp(errmsg);
        }
        public IActionResult TakeContent(int dirId)
        {
            FileDirTakeContentResult res = _fileDirService.TakeContent(dirId);
            if(res is null)
                return this.ApiFailedResp("文件夹内容获取失败");
            return this.ApiResp(res);
        }
        public IActionResult Edit(int id)
        {
            var data = _fileDirService.GetById(id);
            if(data is null)
                return BadRequest();
            FileDirComModel resp = new()
            {
                Id = data.Id,
                Name = data.Name,
                CanEditInfo = true,
                CanPutFile = true
            };
            return this.ApiResp(resp);
        }
        public IActionResult EditExe([FromBody]FileDirComModel req)
        {
            if (req is null)
                return BadRequest();
            if (!_fileDirService.UpdateInfo(req.Id,req.Name, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult PutInFile([FromBody] PutInFileRequest req)
        {
            if(req is null || req.DirPath is null) 
                return BadRequest();
            if (!_fileDirService.MoveFileIn(req.DirPath, req.FileItemId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult PutInThings([FromBody] PutInThingsRequest req)
        {
            if(req is null || req.DirPath is null)
                return BadRequest();
            var res = _fileDirService.MoveThingsIn(req.DirPath,req.FileItemIds,req.FileDirIds,out string? errmsg);
            if (res is null || errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);  
        }

        public class FileDirIndexRequest
        {
            public string[]? Path { get; set; }
            public IndexQuery? Query { get; set; }
        }
        public class PutInFileRequest
        {
            public string[]? DirPath { get; set; }
            public int FileItemId { get; set; }
        }
        public class PutInThingsRequest
        {
            public string[]? DirPath { get; set; }
            public List<int>? FileItemIds { get; set; }
            public List<int>? FileDirIds { get; set; }
        }
        public class FileDirComModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public int Depth { get; set; }
            public bool CanPutFile { get; set; }
            public bool CanEditInfo { get; set; }
        }
    }
}
