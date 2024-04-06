using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Identities
{
    [Authorize]
    //已限制只有所有者才能设置
    public class AuthGrantController : Controller
    {
        private readonly AuthGrantService _authGrantService;
        public AuthGrantController(AuthGrantService authGrantService) 
        {
            _authGrantService = authGrantService;
        }
        
        public IActionResult GetList(AuthGrantOn on,int onId)
        {
            return this.ApiResp(_authGrantService.GetList(on, onId));
        }
        [UserTypeRestricted]
        public IActionResult SetOrder([FromBody]AuthGrantSetOrderRequest req)
        {
            if(!_authGrantService.SetOrder(req.On, req.OnId, req.Ids??new(), out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        [UserTypeRestricted]
        public IActionResult Add([FromBody]AuthGrant req)
        {
            if(!_authGrantService.Add(req, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }
        [UserTypeRestricted]
        public IActionResult Remove(int id)
        {
            if(!_authGrantService.Remove(id, out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }

        public class AuthGrantSetOrderRequest
        {
            public AuthGrantOn On { get; set; }
            public int OnId { get; set; }
            public List<int>? Ids { get; set; }
        }
    }
}
