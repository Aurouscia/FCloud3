using FCloud3.Services.Messages;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Messages
{
    public class OpRecordController : Controller
    {
        private readonly OpRecordService _opRecordService;
        public OpRecordController(OpRecordService opRecordService)
        {
            _opRecordService = opRecordService;
        }

        public IActionResult Get([FromBody]OpRecordGetRequest req)
        {
            return this.ApiResp(_opRecordService.Get(req.Skip, req.User));
        }

        public class OpRecordGetRequest
        {
            public int Skip { get; set; }
            public int User { get; set; }
        }
    }
}
