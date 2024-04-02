using FCloud3.App.Utils;
using FCloud3.Repos;
using FCloud3.Services.Files;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Files
{
    public class MaterialController : Controller
    {
        private readonly MaterialService _materialService;

        public MaterialController(MaterialService materialService)
        {
            _materialService = materialService;
        }
        public IActionResult Index([FromBody] IndexQuery query, bool onlyMine)
        {
            return this.ApiResp(_materialService.Index(query, onlyMine));
        }
        public IActionResult Add([FromBody] MaterialCreateRequest req)
        {
            if (req.MaterialFile is null)
                return BadRequest();
            using Stream stream = req.MaterialFile.OpenReadStream();
            if (!_materialService.Add(stream, ValidFilePathBases.material, req.Name, req.Desc, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult EditContent([FromBody] MaterialContentEditRequest req)
        {
            if (req.MaterialFile is null)
                return BadRequest();
            using Stream stream = req.MaterialFile.OpenReadStream();
            if (!_materialService.UpdateContent(req.Id, stream, ValidFilePathBases.material, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult Delete([FromBody] int id)
        {
            if (!_materialService.Delete(id, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class MaterialCreateRequest
        {
            public string? Name { get; set; }
            public string? Desc { get; set; }
            public IFormFile? MaterialFile { get; set; }
        }
        public class MaterialContentEditRequest
        {
            public int Id { get; set; } 
            public IFormFile? MaterialFile { get; set; }
        }
    }
}
