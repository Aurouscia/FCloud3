using FCloud3.App.Services.Utils;
using FCloud3.Entities.Identities;
using FCloud3.Repos;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FCloud3.Services.Identities.UserService;

namespace FCloud3.App.Controllers.Identities
{
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly HttpUserInfoService _user;

        public UserController(UserService userService,HttpUserInfoService user)
        {
            _userService = userService;
            _user = user;
        }

        public IActionResult Create(string? name,string? pwd)
        {
            if(!_userService.TryCreate(name,pwd,out string? errmsg))
            {
                return this.ApiFailedResp(errmsg);
            }
            return this.ApiResp();
        }

        public IActionResult Index([FromBody]IndexQuery q)
        {
            var res = _userService.Index(q);
            return this.ApiResp(res);
        }

        [Authorize]
        public IActionResult Edit()
        {
            int uid = _user.Id;
            UserComModel? model = _userService.GetById(uid);
            if(model is null)
                return this.ApiFailedResp("找不到指定用户");
            return this.ApiResp(model);
        }

        [Authorize]
        public IActionResult EditExe([FromBody]UserComModel model)
        {
            int uid = _user.Id;
            if (model.Id != uid)
                return this.ApiFailedResp("ID不匹配");
            if (!_userService.TryEdit(uid,model.Name, model.Pwd, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult GetInfoByName(string name)
        {
            UserComModel? u = _userService.GetByName(name);
            if(u is null)
                return this.ApiFailedResp($"找不到用户[{name}]");
            return this.ApiResp(u);
        }

        [Authorize]
        public IActionResult ReplaceAvatar(int id, int materialId)
        {
            if(!_userService.ReplaceAvatar(id, materialId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
    }
}
