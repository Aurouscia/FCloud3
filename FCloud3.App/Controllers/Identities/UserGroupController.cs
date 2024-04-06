using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.Entities.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Identities
{
    [Authorize]
    public class UserGroupController:Controller, IAuthGrantTypeProvidedController
    {
        private readonly UserGroupService _userGroupService;
        private readonly UserService _userService;
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.UserGroup;

        public UserGroupController(UserGroupService userGroupService, UserService userService)
        {
            _userGroupService = userGroupService;
            _userService = userService;
        }

        [UserTypeRestricted]
        public IActionResult Create(string name)
        {
            if(!_userGroupService.Create(name, out string? errmsg))
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult GetList(string? search)
        {
            var res = _userGroupService.GetList(search, out string? errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }
        public IActionResult GetDetail(int id)
        {
            var res = _userGroupService.GetDetail(id, out string? errmsg);
            if(res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }
        public IActionResult Edit(int id)
        {
            var res = _userGroupService.GetById(id, out string? errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult EditExe([FromBody]UserGroupComModel model)
        {
            var res = _userGroupService.EditInfo(model.Id, model.Name, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [AuthGranted(formKey:nameof(groupId))]
        [UserTypeRestricted]
        public IActionResult AddUserToGroup(int groupId, int userId)
        {
            var type = _userService.GetUserType();
            bool needAudit = type != UserType.SuperAdmin;
            var res = _userGroupService.AddUserToGroup(userId, groupId, needAudit, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public IActionResult AnswerInvitation(int groupId, bool accept)
        {
            var res = _userGroupService.AnswerInvitaion(groupId,accept , out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [AuthGranted(formKey: nameof(groupId))]
        [UserTypeRestricted]
        public IActionResult RemoveUserFromGroup(int groupId, int userId)
        {
            var res = _userGroupService.RemoveUserFromGroup(userId, groupId, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult Leave(int groupId)
        {
            var res = _userGroupService.Leave(groupId, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        [AuthGranted]
        [UserTypeRestricted]
        public IActionResult Dissolve(int id)
        {
            var res = _userGroupService.Dissolve(id, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class UserGroupComModel : IAuthGrantableRequestModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }

            public int AuthGrantOnId => Id;
        }
    }
}
