using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Repos;
using FCloud3.Services.Files;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Files
{
    public class FileDirController: Controller, IAuthGrantTypeProvidedController
    {
        private readonly FileDirService _fileDirService;
        private readonly AuthGrantService _authGrantService;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.Dir;

        public FileDirController(FileDirService fileDirService, AuthGrantService authGrantService)
        {
            _fileDirService = fileDirService;
            _authGrantService = authGrantService;
        }

        public IActionResult GetPathById(int id)
        {
            var res = _fileDirService.GetPathById(id);
            if (res is null)
                return this.ApiFailedResp("查找指定id的文件夹时出错");
            return this.ApiResp(res);
        }

        public IActionResult Index([FromBody]FileDirIndexRequest req)
        {
            if (req.Query is null || req.Path is null)
                return BadRequest();
            var res = _fileDirService.GetContent(req.Query, req.Path,out string? errmsg);
            if (res is not null)
            {
                return this.ApiResp(res);
            }
            return this.ApiFailedResp(errmsg);
        }
        public IActionResult Edit(int id)
        {
            var data = _fileDirService.GetById(id);
            if(data is null)
                return BadRequest();
            bool authGranted = _authGrantService.Test(AuthGrantOn.Dir, id);
            FileDirComModel resp = new()
            {
                Id = data.Id,
                Name = data.Name,
                UrlPathName = data.UrlPathName,
                CanEditInfo = authGranted,
                CanPutThings = authGranted,
                CanCreateSub = authGranted
            };
            return this.ApiResp(resp);
        }
        [Authorize]
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult EditExe([FromBody]FileDirComModel req)
        {
            if (req is null)
                return BadRequest();
            if (!_fileDirService.UpdateInfo(req.Id, req.Name, req.UrlPathName, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted(ignoreZero: true)]//检查了操作者在目标文件夹是否有权限，文件有没有权限在service里检查
        [UserTypeRestricted]
        public IActionResult PutInFile([FromBody] PutInFileRequest req)
        {
            if(req is null) 
                return BadRequest();
            if (!_fileDirService.MoveFileIn(req.DirId, req.FileItemId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted(ignoreZero: true)]//检查了操作者在目标文件夹是否有权限，被移动的东西有没有权限在service里检查
        [UserTypeRestricted]
        public IActionResult PutInThings([FromBody] PutInThingsRequest req)
        {
            if(req is null)
                return BadRequest();
            var res = _fileDirService.MoveThingsIn(req.DirId,req.FileItemIds,req.FileDirIds,req.WikiItemIds,out string? errmsg);
            if (res is null || errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);  
        }
        [Authorize]
        [AuthGranted(ignoreZero: true)]//检查了操作者在目标夫文件夹是否有权限
        [UserTypeRestricted]
        public IActionResult Create([FromBody] FileDirCreateRequest req)
        {
            if(!_fileDirService.Create(req.ParentDir, req.Name, req.UrlPathName, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        [Authorize]
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult Delete(int dirId)
        {
            if (!_fileDirService.Delete(dirId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class FileDirIndexRequest
        {
            public string[]? Path { get; set; }
            public IndexQuery? Query { get; set; }
        }
        public class PutInFileRequest : IAuthGrantableRequestModel
        {
            public int DirId { get; set; }
            public int FileItemId { get; set; }
            public int AuthGrantOnId => DirId;
        }
        public class PutInThingsRequest : IAuthGrantableRequestModel
        {
            public int DirId { get; set; }
            public List<int>? FileItemIds { get; set; }
            public List<int>? FileDirIds { get; set; }
            public List<int>? WikiItemIds { get; set; }
            public int AuthGrantOnId => DirId;
        }
        public class FileDirComModel : IAuthGrantableRequestModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? UrlPathName { get; set; }
            public int Depth { get; set; }
            public bool CanPutThings { get; set; }
            public bool CanCreateSub { get; set; }
            public bool CanEditInfo { get; set; }
            public int AuthGrantOnId => Id;
        }
        public class FileDirCreateRequest : IAuthGrantableRequestModel
        {
            public int ParentDir { get; set; }
            public string? Name { get; set; }
            public string? UrlPathName { get; set; }
            public int AuthGrantOnId => ParentDir;
        }
    }
}
