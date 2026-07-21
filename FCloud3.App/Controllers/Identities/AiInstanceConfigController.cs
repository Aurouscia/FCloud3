using FCloud3.App.Models.COM;
using FCloud3.App.Services.Filters;
using FCloud3.Entities.Ai;
using FCloud3.Entities.Identities;
using FCloud3.Services.Ai;
using FCloud3.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Identities
{
    [Authorize]
    public class AiInstanceConfigController(
        AiInstanceConfigService configService,
        AiChatService chatService) : Controller, IAuthGrantTypeProvidedController
    {
        public AuthGrantOn AuthGrantOnType => AuthGrantOn.UserGroup;

        public IActionResult Get(int id)
        {
            var res = configService.GetConfig(id, out var errmsg);
            if (res is null)
                return this.ApiFailedResp(errmsg);
            return this.ApiResp(new AiInstanceConfigModel
            {
                Id = res.Id,
                GroupId = res.GroupId,
                InstanceName = res.InstanceName,
                ApiBaseUrl = res.ApiBaseUrl,
                ApiKeySet = !string.IsNullOrEmpty(res.ApiKey),
                ApiKey = null,
                DefaultModelName = res.DefaultModelName,
                SystemPrompt = res.SystemPrompt,
                Enabled = res.Enabled,
                DefaultDirId = res.DefaultDirId,
                MaxContextMessages = res.MaxContextMessages,
                DailyTokenLimit = res.DailyTokenLimit,
                MonthlyTokenLimit = res.MonthlyTokenLimit
            });
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

        /// <summary>获取指定 API 端点下可用的模型列表</summary>
        [HttpPost]
        public async Task<IActionResult> GetAvailableModels([FromBody] AiAvailableModelsRequest request)
        {
            string apiBaseUrl;
            string apiKey;
            if (request.InstanceId.HasValue)
            {
                var config = configService.GetConfig(request.InstanceId.Value, out var errmsg);
                if (config is null)
                    return this.ApiFailedResp(errmsg);
                apiBaseUrl = config.ApiBaseUrl ?? request.ApiBaseUrl ?? string.Empty;
                apiKey = config.ApiKey ?? string.Empty;
            }
            else
            {
                apiBaseUrl = request.ApiBaseUrl ?? string.Empty;
                apiKey = request.ApiKey ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(apiBaseUrl) || string.IsNullOrWhiteSpace(apiKey))
                return this.ApiFailedResp("API 地址和 Key 不能为空");

            var (models, errmsg2) = await chatService.GetAvailableModels(apiBaseUrl, apiKey);
            if (models is null)
                return this.ApiFailedResp(errmsg2 ?? "无法获取模型列表，请检查 API 地址和 Key");
            return this.ApiResp(new { Models = models });
        }

        [UserTypeRestricted]
        public IActionResult Set([FromBody] AiInstanceConfigModel model)
        {
            var config = new AiInstanceConfig
            {
                Id = model.Id,
                GroupId = model.GroupId,
                InstanceName = model.InstanceName,
                ApiBaseUrl = model.ApiBaseUrl,
                ApiKey = model.ApiKey,
                DefaultModelName = model.DefaultModelName,
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
            public string? InstanceName { get; set; }
            public string? ApiBaseUrl { get; set; }
            public string? ApiKey { get; set; }
            public bool ApiKeySet { get; set; }
            public string? DefaultModelName { get; set; }
            public string? SystemPrompt { get; set; }
            public bool Enabled { get; set; }
            public int DefaultDirId { get; set; }
            public int MaxContextMessages { get; set; }
            public int DailyTokenLimit { get; set; }
            public int MonthlyTokenLimit { get; set; }
            public int AuthGrantOnId => GroupId;
        }

        public class AiAvailableModelsRequest
        {
            public int? InstanceId { get; set; }
            public string? ApiBaseUrl { get; set; }
            public string? ApiKey { get; set; }
        }
    }
}
