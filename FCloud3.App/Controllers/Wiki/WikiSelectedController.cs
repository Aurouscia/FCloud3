using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Wiki
{
    [Authorize]
    public class WikiSelectedController(
        WikiSelectedService wikiSelectedService
        ) : Controller
    {
        [AllowAnonymous]
        public IActionResult GetList()
        {
            var res = wikiSelectedService.GetList();
            return this.ApiResp(res);
        }

        [UserTypeRestricted(UserType.Admin)]
        public IActionResult Insert(WikiSelectedDto model)
        {
            var res = wikiSelectedService.Insert(model.Order, model, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        
        [UserTypeRestricted(UserType.Admin)]
        public IActionResult Edit(WikiSelectedDto model)
        {
            var res = wikiSelectedService.Edit(model, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        
        [UserTypeRestricted(UserType.Admin)]
        public IActionResult Remove(int id)
        {
            var res = wikiSelectedService.Remove(id, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}