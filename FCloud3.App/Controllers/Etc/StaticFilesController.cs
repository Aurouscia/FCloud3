using FCloud3.App.Utils;
using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.AspNetCore.Mvc;
using MimeMapping;

namespace FCloud3.App.Controllers.Etc
{
    public class StaticFilesController : Controller
    {
        private readonly IStorage _storage;

        public StaticFilesController(IStorage storage)
        {
            _storage = storage;
        }

        [Route($"/{{pathBase:{FilePathBaseConstraint.constraintName}}}/{{fileName}}")]
        public IActionResult Get(string pathBase, string fileName)
        {
            string pathName = $"{pathBase}/{fileName}";
            if (_storage.ProvideType == StorageProvideType.Stream)
            {
                var stream = _storage.Read(pathName);
                if (stream is null)
                    return NotFound();
                return File(stream, MimeUtility.GetMimeMapping(fileName));
            }
            else if (_storage.ProvideType == StorageProvideType.Redirect)
            {
                var url = _storage.FullUrl(pathName);
                return RedirectPermanent(url);
            }
            throw new Exception("未知静态文件提供方式");
        }
    }
}
