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

        public IActionResult Get(int id)
        {
            var res = configService.GetConfig(id, out var errmsg);
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

        [AuthGranted(formKey: nameof(groupId))]
        public IActionResult GetList(int groupId)
        {
            var list = configService.GetListByGroupId(groupId);
            return this.ApiResp(list);
        }

        [UserTypeRestricted]
        public IActionResult Set([FromBody] AiInstanceConfigModel model)
        {
            var config = new AiInstanceConfig
            {
                Id = model.Id,
                GroupId = model.GroupId,
                ApiBaseUrl = model.ApiBaseUrl,
                ApiKey = model.ApiKey,
                ModelName = model.ModelName,
                SystemPrompt = model.SystemPrompt,
                Enabled = model.Enabled,
                DefaultDirId = model.DefaultDirId,
                MaxContextMessages = model.MaxContextMessages,
                DailyTokenLimit = model.DailyTokenLimit,
                MonthlyTokenLimit = model.MonthlyTokenLimit
            };
            var res = configService.SetConfig(config, out var errmsg);
            if (!res)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp();
        }

        public class AiInstanceConfigModel : IAuthGrantableRequestModel
        {
            public int Id { get; set; }
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
