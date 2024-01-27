using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Wiki
{
    public class WikiParaController : Controller
    {
        private readonly WikiParaService _wikiParaService;

        public WikiParaController(WikiParaService wikiParaService)
        {
            _wikiParaService = wikiParaService;
        }
        public IActionResult SetFileParaFileId(int paraId, int fileId)
        {
            if(!_wikiParaService.SetFileParaFileId(paraId, fileId, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
    }
}
