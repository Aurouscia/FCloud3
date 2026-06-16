using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.Entities.Ai;
using FCloud3.Entities.Identities;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Identities
{
    [Authorize]
    public class AiInstanceConfigController(
        AiInstanceConfigService configService) : Controller, IAuthGrantTypeProvidedController
    {
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.UserGroup;

        [AuthGranted(formKey: nameof(groupId))]
        public IActionResult Get(int groupId)
        {
            var res = configService.GetConfig(groupId, out var errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(res);
        }

        /// <summary>获取当前用户可用的 AI 实例配置列表</summary>
        public IActionResult GetMyAvailableInstances()
        {
            var list = configService.GetMyAvailableInstances();
            return this.ApiResp(list);
        }

        [AuthGranted(formKey: nameof(model.GroupId))]
        [UserTypeRestricted]
        public IActionResult Set([FromBody] AiInstanceConfigModel model)
        {
            var res = configService.SetConfig(model.GroupId, model.ApiBaseUrl!,
                model.ApiKey!, model.ModelName!, model.SystemPrompt, model.Enabled,
                model.DefaultDirId, model.MaxContextMessages, model.DailyTokenLimit,
                model.MonthlyTokenLimit, out var errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class AiInstanceConfigModel : IAuthGrantableRequestModel
        {
            public int GroupId { get; set; }
            public string? ApiBaseUrl { get; set; }
            public string? ApiKey { get; set; }
            public string? ModelName { get; set; }
            public string? SystemPrompt { get; set; }
            public bool Enabled { get; set; }
            public int DefaultDirId { get; set; }
            public int MaxContextMessages { get; set; }
            public int DailyTokenLimit { get; set; }
            public int MonthlyTokenLimit { get; set; }
            public int AuthGrantOnId => GroupId;
        }
    }
}
