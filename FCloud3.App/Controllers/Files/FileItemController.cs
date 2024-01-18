using FCloud3.Services.Files;
using FCloud3.Utils.Utils.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Files
{
    public class FileItemController : Controller
    {
        private readonly IFileItemService _fileService;
        private const int maxUploadLength = 10 * 1000 * 1000;

        public FileItemController(IFileItemService fileService)
        {
            _fileService = fileService;
        }
        public IActionResult Save(FileUploadRequest request)
        {
            if (request is null || request.ToSave is null)
                return BadRequest();
            if (request.StorePath is null || !ValidPaths.Contains(request.StorePath))
                return this.ApiFailedResp("不支持的StorePath");
            if (request.DisplayName is null)
                return this.ApiFailedResp("请填写文件显示名");
            if (request.ToSave.Length > maxUploadLength)
                return this.ApiFailedResp("文件过大，请压缩或分开上传");
            int id = _fileService.Save(
                stream: request.ToSave.OpenReadStream(),
                byteCount: (int)request.ToSave.Length,
                displayName: request.DisplayName,
                storePath: request.StorePath,
                storeName: null, out string? errmsg);
            if (id == 0)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new {CreatedId = id});
        }

        private static List<string> ValidPaths = new List<string> { "upload", "wikiFile", "material", "forum", "test" };
        public class FileUploadRequest
        {
            public IFormFile? ToSave { get; set; }
            public string? DisplayName { get; set; }
            public string? StorePath { get; set; }
            public string? StoreName { get; set; }
        }
    }
}
