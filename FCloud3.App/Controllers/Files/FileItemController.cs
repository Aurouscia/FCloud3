using FCloud3.App.Services.Filters;
using FCloud3.App.Utils;
using FCloud3.Entities.Identities;
using FCloud3.Services.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace FCloud3.App.Controllers.Files
{
    public class FileItemController : Controller, IAuthGrantTypeProvidedController
    {
        private readonly FileItemService _fileService;
        private const int maxUploadLength = 10 * 1000 * 1000;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.FileItem;

        public FileItemController(FileItemService fileService)
        {
            _fileService = fileService;
        }

        public IActionResult GetDetail(int id)
        {
            var detail = _fileService.GetDetail(id, out string? errmsg);
            if (detail is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(detail);
        }
        [Authorize]
        [UserTypeRestricted]
        [AuthGranted(nameof(id))]
        public IActionResult EditInfo(int id, string name)
        {
            var res = _fileService.EditInfo(id, name, out string? errmsg);
            if(!res || errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [Authorize]
        [UserTypeRestricted]
        [AuthGranted]
        public IActionResult Delete(int id)
        {
            var res = _fileService.Delete(id, out string? errmsg);
            if (!res || errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [Authorize]
        [UserTypeRestricted]
        [UserActiveOperation]
        public IActionResult Save(FileUploadRequest request)
        {
            if (request is null || request.ToSave is null)
                return BadRequest();
            if (request.StorePath is null || !ValidFilePathBases.Contains(request.StorePath))
                return this.ApiFailedResp("不支持的StorePath");
            if (request.DisplayName is null)
                return this.ApiFailedResp("请填写文件显示名");
            if (request.ToSave.Length > maxUploadLength)
                return this.ApiFailedResp("文件过大，请压缩或改为放网盘链接");
            int id = _fileService.Save(
                stream: request.ToSave.OpenReadStream(),
                byteCount: (int)request.ToSave.Length,
                displayName: request.DisplayName,
                storePath: request.StorePath,
                storeName: null,
                hash: request.Hash,
                out string? errmsg);
            if (id <= 0)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new {CreatedId = id});
        }

        /// <summary>
        /// 下载指定文件并保留上传时的名称，文件和网页不在同一域名下时，download标签会被无视
        /// 必须通过服务端转发stream才能附上文件名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Download(int id)
        {
            var res = _fileService.Read(id, out string? errmsg);
            if(res is not null)
            {
                return File(res.Stream, Application.Octet, res.DownloadName);
            }
            return this.ApiFailedResp(errmsg);
        }

        public class FileUploadRequest
        {
            public IFormFile? ToSave { get; set; }
            public string? DisplayName { get; set; }
            public string? StorePath { get; set; }
            public string? StoreName { get; set; }
            public string? Hash { get; set; }
        }
    }
}
