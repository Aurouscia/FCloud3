using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc.Index;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FCloud3.Services.Identities.UserService;

namespace FCloud3.App.Controllers.Identities
{
    public class UserController : Controller, IAuthGrantTypeProvidedController
    {
        private readonly UserService _userService;
        private readonly HttpUserInfoService _user;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.User;

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
        [AuthGranted]
        public IActionResult EditExe([FromBody]UserComModelRequest model)
        {
            if (!_userService.TryEdit(model.Id ,model.Name, model.Pwd, out string? errmsg))
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
        [AuthGranted(nameof(id))]
        public IActionResult ReplaceAvatar(int id, int materialId)
        {
            if(!_userService.ReplaceAvatar(id, materialId, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        [Authorize]
        [UserTypeRestricted(UserType.Admin)]
        public IActionResult SetUserType(int id, UserType type)
        {
            var currentUserType = _user.Type;
            if (!_userService.SetUserType(id, type, currentUserType, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult GetInfo(int id)
        {
            UserComModel? model = _userService.GetById(id);
            if (model is null)
                return this.ApiFailedResp("找不到指定用户");
            return this.ApiResp(model);
        }

        public class UserComModelRequest : UserComModel, IAuthGrantableRequestModel
        {
            public int AuthGrantOnId => Id;
        }
    }
}
