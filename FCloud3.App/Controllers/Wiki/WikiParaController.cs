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

        [Authorize]
        [UserTypeRestricted]
        [AuthGranted(nameof(paraId))]
        public IActionResult SetInfo(int paraId, string? nameOverride)
        {
            if (string.IsNullOrWhiteSpace(nameOverride))
            {
                nameOverride = null;
            }
            if (!_wikiParaService.SetInfo(paraId, nameOverride, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }

        public IActionResult ViewParaRawContent(int paraId)
        {
            var res = _wikiParaService.GetParaRawContent(paraId, out string? errmsg);
            if(errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }

        [Authorize]
        [UserTypeRestricted]
        [AuthGranted(nameof(paraId))]
        public IActionResult ConvertXlsxToAuTable(int paraId)
        {
            var success = _wikiParaService.ConvertXlsxToAuTable(paraId, out string? errmsg);
            if (errmsg is not null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}
