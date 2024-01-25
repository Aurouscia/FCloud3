using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FCloud3.App.Controllers.Identities
{
    public class UserGroupController:Controller
    {
        private readonly UserGroupService _userGroupService;

        public UserGroupController(UserGroupService userGroupService)
        {
            _userGroupService = userGroupService;
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
        public IActionResult EditExe([FromBody]UserGroupComModel model)
        {
            var res = _userGroupService.EditInfo(model.Id, model.Name, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }
        public IActionResult AddUserToGroup(int userId, int groupId)
        {
            var res = _userGroupService.AddUserToGroup(userId, groupId, out string? errmsg);
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
        public IActionResult RemoveUserFromGroup(int userId, int groupId)
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
        public IActionResult Dissolve(int id)
        {
            var res = _userGroupService.Dissolve(id, out string? errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class UserGroupComModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
    }
}
