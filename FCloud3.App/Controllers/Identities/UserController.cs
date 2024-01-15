using FCloud3.App.Services;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [Authorize]
        public IActionResult Edit()
        {
            int uid = _user.Id;
            User u = _userService.GetById(uid) ?? throw new Exception("找不到指定ID的用户");
            UserComModel model = UserComModel.ExcludePwd(u);
            return this.ApiResp(model);
        }

        [Authorize]
        public IActionResult EditExe([FromBody]UserComModel model)
        {
            int uid = _user.Id;
            if (model.Id != uid)
                throw new Exception("无权限");
            if (!_userService.TryEdit(uid,model.Name, model.Pwd, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult GetInfoByName(string name)
        {
            User? u = _userService.GetByName(name);
            if(u is null)
            {
                return this.ApiFailedResp($"找不到用户[{name}]");
            }
            UserComModel model = UserComModel.ExcludePwd(u);
            return this.ApiResp(model);
        }

        /// <summary>
        /// 对应前端：\src\models\identities\user.ts
        /// </summary>
        public class UserComModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Pwd { get; set; }
            public string? AvatarFileName { get; set; }

            public static UserComModel ExcludePwd(User u)
            {
                return new UserComModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    AvatarFileName = u.AvatarFileName
                };
            }
        }
    }
}
