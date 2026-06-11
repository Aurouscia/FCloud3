using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Ai
{
    [Authorize]
    public class AiUsageController(
        AiUsageService usageService,
        HttpUserInfoService httpUserInfoService) : Controller
    {
        /// <summary>获取当前用户在指定AI实例的今日用量</summary>
        public IActionResult MyUsage(int aiInstanceConfigId)
        {
            var userId = httpUserInfoService.Id;
            var total = usageService.GetUserConfigUsage(userId, aiInstanceConfigId);
            return this.ApiResp(new { TotalTokens = total });
        }

        /// <summary>团体管理员查看团体用量排行</summary>
        [AuthGranted(formKey: nameof(groupId))]
        public IActionResult GroupRanking(int groupId)
        {
            var ranking = usageService.GetGroupUsageRanking(groupId);
            return this.ApiResp(ranking);
        }
    }
}
