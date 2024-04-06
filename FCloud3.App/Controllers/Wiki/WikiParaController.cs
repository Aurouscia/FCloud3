using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Wiki
{
    public class WikiParaController : Controller, IAuthGrantTypeProvidedController
    {
        private readonly WikiParaService _wikiParaService;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.WikiPara;

        public WikiParaController(WikiParaService wikiParaService)
        {
            _wikiParaService = wikiParaService;
        }

        [Authorize]
        [UserTypeRestricted]
        [AuthGranted(nameof(paraId))]
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
